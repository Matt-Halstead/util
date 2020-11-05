<Query Kind="Program">
  <Connection>
    <ID>96aebd64-c7ff-4293-b35c-0fd424059f68</ID>
    <Persist>true</Persist>
    <Server>appsql</Server>
    <Database>AR</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

//
// PROBLEMS
//
// Cant seem to get the active database to change.
// The .Open() call does not alter the context, so then getting the tables always returns the same set of tables.
//

void Main()
{
    var serverName = this.Connection.DataSource;
    var databases = ExecuteQuery<String>("SELECT name FROM sys.databases").ToList();

    var ignoredDBs = new[]
    {
        "master",
        "tempdb",
        "model",
        "msdb",
        "SysproCompanyB",
        "Advanced Tracker",
        "Jira",
        "Bitbucket",
        "msdb",
        "Avanti",
        "MasterCatalog",
        "Sysprodb_CompanyA_New_Port39",
        "SysproCompanyA_New_Port39"        
    };
    
    var colSearchList = new[] { "email", "recip" };
    
    var dbInfoLookup = new Dictionary<string, DatabaseInfo>();
    var fieldInfo = new List<FieldInfo>();
        
    foreach (var databaseName in databases.Where(db => !ignoredDBs.Contains(db, StringComparer.OrdinalIgnoreCase)))
    {
        Console.WriteLine(databaseName);
        
        string newConnectionString = $"Data Source={serverName};Integrated Security=SSPI;Initial Catalog={databaseName};app=LINQPad";
        var db = new TypedDataContext(newConnectionString);
        db.Connection.Open();

        var dbInfo = GetDatabaseInfo(db);
        dbInfoLookup.Add(databaseName, dbInfo);

        foreach (var table in dbInfo.Tables)
        {
            foreach (var col in table.Columns)
            {
                foreach (var hit in colSearchList)
                {
                    // Note all email-related fields
                    if (col.ColumnName.ToLower().Contains(hit.ToLower()))
                    {
                        fieldInfo.Add(new FieldInfo { DB = dbInfo, Table = table, Col = col });
                    }
                }
            }
        }
        
        db.Connection.Close();
    }
    
    
    // Output summary of all the email fields
    Console.WriteLine($"=== Listing all email-related fields ===\n");

    fieldInfo
        .GroupBy(fi => new { fi.DB.DatabaseName, fi.Table.TableName })
        .SelectMany(g => g.Select(v => $"{v.DB.DatabaseName}.dbo.{v.Table.TableName} [{v.Col.ColumnName}]"))
        .ToList()
        .ForEach(line => Console.WriteLine(line));

    var renamesLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Kelowna Warehouse", "KelWarehouse@trailappliances.com" },
        { "Purchasing", "purchasing@trailappliances.com" },
        { "RichardB", "Richard Broderick" },
        { "BrentL", "Brent Laturnus" },
        { "StephanieK", "Stephanie Krzyz" },
        { "JasonB", "Jason Broderick" },
        { "TrevorL", "Trevor Love" },
        { "LarryL", "Larry Law" }
    };

//    foreach (var db in fieldInfo
//        .GroupBy(fi => new { fi.DB.DatabaseName, fi.Table.TableName }))
//    {
//        foreach (var table in db.Tables.Where(t => t.Fields.Any()))
//        {
//            var whereClauses = new List<string>();
//            foreach (var field in table.Fields)
//            {
//                foreach (var key in renamesLookup.Keys)
//                {
//                    whereClauses.Add($"lower([{field}]) LIKE lower('%{key}%')");
//                }
//            }
//
//            try
//            {
//                var query = $@"SELECT * FROM {db.Name}.{table.Name}
//WHERE {(string.Join("\n  OR ", whereClauses.ToArray()))};";
//                var rows = ExecuteQueryDynamic(query).ToArray();
//                if (rows.Any())
//                {
//                    //var filename = $"{db.Name}.{table.Name}.csv";
//                    
//                    
//
//                    Console.WriteLine($"{db.Name}.{table.Name}\n{query}\n");
//                    
//                    // updatesList = empty list
//                    var updatesList = new List<Tuple<string, string, string>>();                    
//                    
//                    // dataTable = make a DataTable from query
//                    var row = rows.First();
//                    var t = row as IDataRecord;
//                    
//                    // foreach row in dataTable
//                        // foreach column in row
//                            // foreach pair in renamesLookup
//                                // replace in cell value: pair.Value with ""
//                                // newCellValue = replace in cell value: pair.Key with pair.Value
//                                // add to updatesList: {row, column, newCellValue}
//
//                    // if updatesList.Any()
//                        // output SQL update statement start
//                        // foreach update in updatesList
//                            // row = dataTable.getRow(update.Row)
//                            // rowUpdateStatement = ""
//                            // foreach column in row
//                                // add cell value to rowUpdateStatement                            
//                            // output SQL rowUpdateStatement
//                        // output SQL update statement end
//                    
//                    break;
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine($"ERROR in {db.Name}.{table.Name}: {e.Message}");
//            }
//        }
//    }
}

// Define other methods and classes here
public class DatabaseInfo
{
    public Type DataContextType { get; set; }
    public string DatabaseName { get; set; }
    public string DatabaseServer { get; set; }
    public TableInfo[] Tables { get; set; }
}

public class TableInfo
{
    public Type TableType { get; set; }
    public Type EntityType { get; set; }
    public string TableName { get; set; }
    public ColumnInfo[] Columns { get; set; }
}

public class ColumnInfo
{
    public string ColumnName { get; set; }
    public string DatabaseType { get; set; }
}

public DatabaseInfo GetDatabaseInfo(LINQPad.DataContextBase dataContext)
{
    return new DatabaseInfo
    {
        DatabaseName = dataContext.Connection.Database,
        DatabaseServer = dataContext.Connection.DataSource,
        DataContextType = dataContext.GetType(),
        Tables = GetTables(dataContext.GetType()),
    };
}

public TableInfo[] GetTables(Type dataContextType)
{
    var tableInfoQuery =
        from prop in dataContextType.GetProperties()
        where prop.PropertyType.IsGenericType
        where prop.PropertyType.GetGenericTypeDefinition() == typeof(Table<>)
        let tableType = prop.PropertyType
        let entityType = tableType.GenericTypeArguments.Single()
        select new TableInfo
        {
            TableName = GetTableNameFromEntityType(entityType),
            EntityType = entityType,
            TableType = tableType,
            Columns = GetColumnsFromEntityType(entityType),
        };

    return tableInfoQuery.ToArray();
}

public string GetTableNameFromEntityType(Type entityType)
{
    var tableNameQuery =
        from ca in entityType.CustomAttributes
        from na in ca.NamedArguments
        where na.MemberName == "Name"
        select na.TypedValue.Value.ToString();

    return tableNameQuery.Single();
}

public ColumnInfo[] GetColumnsFromEntityType(Type entityType)
{
    var columnInfoQuery =
        from field in entityType.GetFields()
        from attribute in field.CustomAttributes
        from namedArgument in attribute.NamedArguments
        where namedArgument.MemberName == "DbType"
        select new ColumnInfo
        {
            ColumnName = field.Name,
            DatabaseType = namedArgument.TypedValue.Value.ToString(),
        };

    return columnInfoQuery.ToArray();
}

public class FieldInfo
{
    public DatabaseInfo DB { get; set; }
    public TableInfo Table { get; set; }
    public ColumnInfo Col { get; set; }
}