<Query Kind="Program">
  <Connection>
    <ID>803b02c6-9d1e-42ce-ad5d-4ee099635351</ID>
    <Persist>true</Persist>
    <Server>trailsql</Server>
    <Database>TrailX</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

void Main()
{
	const string queryString = @"
SELECT AppName, DBFullName, Connect
FROM [TrailX].[dbo].[vAccessDBObjects]";

	var allRows = ExecuteQueryDynamic(queryString)
		.Where(row => !string.IsNullOrEmpty(row.AppName))
		.Select(row => new
			{
				AppName = row.AppName.ToString().Trim(),
				DBFullName = row.DBFullName.ToString().Trim(),
				Connect = row.Connect.ToString().Trim(),
				ConnectDB = GetDatabaseName(row.Connect.ToString().Trim())
			})
		.OrderBy(row => row.AppName)
		.ThenBy(row => row.DBFullName)
		.ToArray();
		
	var rowsByName = allRows
		.GroupBy(row => row.AppName)
		.ToArray();

	var rowsByDatabase = allRows
		.GroupBy(row => row.ConnectDB)
		.ToArray();
		
	var csv = ToCSV(
		"AppName, ConnectDB, DBFullName",
		allRows,
		row => new string[] { row.AppName, row.ConnectDB, row.DBFullName });
		
	csv.Dump();

	Console.WriteLine($"ALL ROWS BY APP");
	Console.WriteLine($"===============\n");
	foreach (var group in rowsByName.ToList())
	{
		if (!string.IsNullOrEmpty(group.Key))
		{
			var databases = group
				.Select(row => row.ConnectDB)
				.Where(v => !string.IsNullOrEmpty(v))
				.Distinct();

			Console.WriteLine($"App:  {group.Key}");
			foreach (var db in databases.Distinct())
			{
				Console.WriteLine($"    {db}");
			}
			Console.WriteLine($"");
		}
	}

	Console.WriteLine($"\nALL ROWS BY DATABASE");
	Console.WriteLine($"====================\n");
	foreach (var group in rowsByDatabase.ToList())
	{
		if (!string.IsNullOrEmpty(group.Key))
		{
			var databases = group
				.Select(row => row.AppName)
				.Where(v => !string.IsNullOrEmpty(v))
				.Distinct();

			Console.WriteLine($"Database:  {group.Key}");
			foreach (var db in databases.Distinct())
			{
				Console.WriteLine($"    {db}");
			}
			Console.WriteLine($"");
		}
	}
}

private static Dictionary<string, string> SplitConnectionString(string connection)
{
	var result = new Dictionary<string, string>();
	
	if (!string.IsNullOrEmpty(connection))
	{
		foreach (var pair in connection.Split(';'))
		{
			var tokens = pair.Split('=');
			if (tokens.Length == 2)
			{
				result.Add(tokens[0], tokens[1]);
			}
		}
	}
	
	return result;
}

private static string GetDatabaseName(Dictionary<string, string> connectionStringElements)
{
	return connectionStringElements.TryGetValue("DATABASE", out string dbName) ? dbName : string.Empty;
}

private static string GetDatabaseName(string connectionString)
{
	var map = SplitConnectionString(connectionString);
	return GetDatabaseName(map);
}

private static string ToCSV<TRow>(string header, IEnumerable<TRow> rows, Func<TRow, string[]> toLineFunc)
{
	var sb = new StringBuilder();
	
	sb.AppendLine(header);

	foreach (var row in rows)
	{
		var line = string.Join(", ", toLineFunc(row));
		sb.AppendLine(line);
	}
	
	return sb.ToString();
}
