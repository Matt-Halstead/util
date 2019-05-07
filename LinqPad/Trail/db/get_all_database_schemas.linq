<Query Kind="Program">
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft SQL Server\140\DTS\Tasks\Microsoft.SqlServer.ConnectionInfo.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft SQL Server\140\DTS\Tasks\Microsoft.SqlServer.Smo.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.dll</Reference>
  <Namespace>Microsoft.SqlServer.Management.Common</Namespace>
  <Namespace>Microsoft.SqlServer.Management.Smo</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.IO</Namespace>
</Query>

void Main()
{
	// Open connection to the database
	string conString = "Server=TRAILSQL;App=LinqPad;Trusted_Connection=True;";

	using (SqlConnection con = new SqlConnection(conString))
	{
		con.Open();
		
		//
		// TODO
		//
		// This approach is awfully slow, probably hurts everyone connected.  Do it better.
		//

		var server = new Server(new ServerConnection(con));
		foreach (var database in server.Databases.OfType<Database>())
		{
			Console.WriteLine($"Database: {database.Name}");
			foreach (var table in database.Tables.OfType<Table>())
			{
				Console.WriteLine($"	Table: {table.Name}");

				foreach (var column in table.Columns.OfType<Column>())
				{
					Console.WriteLine($"		Column: {column.Name} [{column.DataType}]");
				}
				
				foreach (var fk in table.ForeignKeys.OfType<ForeignKey>())
				{
					Console.WriteLine($"		FK: {fk.Name} --> {fk.ReferencedTableSchema}.{fk.ReferencedTable}");
				}
			}
		}
	}
}

public static List<string> GetDatabaseList()
{
	List<string> list = new List<string>();

	// Open connection to the database
	string conString = "Server=TRAILSQL;App=LinqPad;Trusted_Connection=True;";

	using (SqlConnection con = new SqlConnection(conString))
	{
		con.Open();

		// Set up a command with the given query and associate
		// this with the current connection.
		using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
		{
			using (IDataReader dr = cmd.ExecuteReader())
			{
				while (dr.Read())
				{
					list.Add(dr[0].ToString());
				}
			}
		}
	}
	return list;
}
