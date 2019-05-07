<Query Kind="Program">
  <Connection>
    <ID>6b7b0503-ec95-452f-a55e-1a7eb5ed93d3</ID>
    <Persist>true</Persist>
    <Server>trailusql\util</Server>
    <Database>development</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <Output>DataGrids</Output>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Data</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

void Main()
{
	var sqlSmall = @"SELECT EventId, Logger FROM [development].[legacy].[Launcher]";
	
	using (var connection = new SqlConnection("Server=TRAILUSQL\\UTIL;Database=development;User ID=nlog_user;Password=TrailLog123;App=Matt's LINQPAD query"))
	{
		DatabaseUtil.QueryValue(connection, sqlSmall).Dump();
		DatabaseUtil.QueryRow(connection, sqlSmall)["Logger"].Dump();

		foreach (var row in DatabaseUtil.QueryRows(connection, sqlSmall))
		{
			$"Row: {row["EventId"]}".Dump();
		}
	}
}

    public class DatabaseUtil
    {
        public static readonly string AppTitle = Assembly.GetEntryAssembly().GetName().Name;
        public static readonly string SystemUser = Environment.UserName.ToUpper().Trim();
        public static readonly string SystemMachineName = Environment.MachineName.ToUpper().Trim();

        public static string ModifyConnectionString(string connectionString, string dataSource = "", string tableName = "", string applicationName = "")
        {
            var builder = new SqlConnectionStringBuilder(connectionString ?? string.Empty);
            if (!string.IsNullOrEmpty(dataSource))
            {
                builder["Data Source"] = dataSource;
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                builder["Initial Catalog"] = tableName;
            }

            if (!string.IsNullOrEmpty(applicationName))
            {
                builder["Application Name"] = applicationName;
            }

            return builder.ConnectionString;
        }

        /// <summary>
        /// Executes the given sql statement using the connection and attempts to return the first value from reader.
        /// </summary>
        /// <param name="connection">the database connection</param>
        /// <param name="sql">sql to be executed</param>
        /// <returns>first row object if any results, or null if no result or error.</returns>
        public static object QueryValue(SqlConnection connection, string sql)
        {
            var row = QueryRow(connection, sql);
            return row != null ? row[0] : null;
        }

        public static IDataRecord QueryRow(SqlConnection connection, string sqlQuery)
        {
            var result = Enumerable.Empty<IDataRecord>();

            try
            {
                ExecSqlCommand<bool, object>(connection, sqlQuery, null, reader => result = reader.Cast<IDataRecord>().Take(1).ToList(), CommandBehavior.SingleRow);
            }
            catch (Exception e)
            {
                string msg = $@"Error: {e.Message}

In lookup

{sqlQuery}";
                MessageBox.Show(msg, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result?.FirstOrDefault();
        }

        public static IEnumerable<IDataRecord> QueryRows(SqlConnection connection, string sqlQuery)
        {
            var result = Enumerable.Empty<IDataRecord>();

            try
            {
                ExecSqlCommand<bool, object>(connection, sqlQuery, null, reader => result = reader.Cast<IDataRecord>().ToList());
            }
            catch (Exception e)
            {
                string msg = $@"Error: {e.Message}

In lookup

{sqlQuery}";
                MessageBox.Show(msg, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result;
        }

        /// <summary>
        /// Executes a 'select *' in given table with optional where statement using the connection and returns whether or not any rows are returned.
        /// </summary>
        /// <param name="connection">the database connection</param>
        /// <param name="tableName">name of table to query</param>
        /// <param name="whereStatement">where clause to append to sql statement</param>
        /// <returns>true if statement is executed succesfully and returns >= 1 result rows</returns>
        public static bool lsrRse(SqlConnection connection, string tableName, string whereStatement)
        {
            string whereClause = string.IsNullOrEmpty(whereStatement) ? string.Empty : $" Where {whereStatement}";
            string sql = $"Select * from {tableName}{whereClause}";

            return lsrRse(connection, sql);
        }

        /// <summary>
        /// Executes the given sql using the connection and returns true if NO rows are returned, else false if there ARE rows.
        /// </summary>
        /// <param name="connection">the database connection</param>
        /// <param name="sql">sql statement to execute</param>
        /// <returns>true if statement is executed succesfully and returns 0 result rows</returns>
        public static bool lsrRse(SqlConnection connection, string sql)
        {
            bool result = true;

            try
            {
                ExecSqlCommand<bool, bool>(connection, sql, null, reader => result = !reader.HasRows, CommandBehavior.SingleRow);
            }
            catch (Exception e)
            {
                string msg = $@"Error in RSE: {e.Message}

In RSE

{sql}";
                MessageBox.Show(msg, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result;
        }

        /// <summary>
        /// Executes the given sql statement.  No rows are returned.
        /// </summary>
        /// <param name="connection">the database connection</param>
        /// <param name="sql">sql statement to execute</param>
        /// <param name="showError">flag controlling whether errors are reported</param>
        /// <returns>true if statement is executed succesfully</returns>
        public static bool ExecuteSql(SqlConnection connection, string sql, bool showError)
        {
            bool result = false;

            try
            {
                ExecSqlCommand<int, bool>(connection, sql, command => command.ExecuteNonQuery());
                result = true;
            }
            catch (Exception e)
            {
                if (showError)
                {
                    string msg = $@"Error:  {e.Message}

While executing

{sql}";
                    MessageBox.Show(msg, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the given sql statement using connection, and returns the last identity value generated for named table.
        /// </summary>
        /// <param name="connection">the database connection</param>
        /// <param name="sql">sql statement to execute</param>
        /// <param name="tableName">name of table to query</param>
        /// <param name="showError">flag controlling whether errors are reported</param>
        /// <returns></returns>
        public static long ExecuteSqlGetIdent(SqlConnection connection, string sql, string tableName, bool showError)
        {
            long result = 0;

            try
            {
                ExecSqlCommand(connection, sql, command =>
                {
                    int commandResult = command.ExecuteNonQuery();
                    command.CommandText = $"Select IDENT_CURRENT('{tableName}')";
                    return commandResult;
                },
                reader => result = Convert.ToInt64(reader[0]),
                CommandBehavior.SingleRow);
            }
            catch (Exception e)
            {
                if (showError)
                {
                    string msg = $@"Error:  {e.Message}

While executing

{sql}";
                    MessageBox.Show(msg, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            return result;
        }

        /// <summary>
        /// Escapes the given text so that it is displayed properly when opened as html/xml (i.e. an "&" is converted to "&amp;")
        /// </summary>
        /// <param name="text">input text</param>
        /// <returns>escaped text</returns>
        public static string EscapeXml(string text)
        {
            string result = text ?? string.Empty;
            result = result.Replace("&", "and");
            result = result.Replace("<", "&lt;");
            result = result.Replace(">", "&gt;");
            return result;
        }

        /// <summary>
        /// Double any single quote (') characters in the given text and return result.
        /// </summary>
        /// <param name="text">text to be processed</param>
        /// <returns>result of double quoting the given text</returns>
        public static string EscapeSingleQuotes(string text)
        {
            var result = text?.Trim() ?? string.Empty;
            result = result.Replace("'", "''");
            return result;
        }

        // Create a connection, create a command from given sql, execute the given execCommandAction.
        // If processReadResultsFunc is also set, an SqlDataReader is created for the command before being sent to processReadResultsFunc
        // so that the caller can extract the results it needs.
        private static void ExecSqlCommand<TCommand, TRow>(SqlConnection connection, string sql, Func<SqlCommand, TCommand> execCommandAction = null, Func<SqlDataReader, TRow> processReadResultsFunc = null, CommandBehavior commandBehavior = CommandBehavior.CloseConnection)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                execCommandAction?.Invoke(command);

			if (processReadResultsFunc != null)
			{
				var sqlDataReader = command.ExecuteReader(commandBehavior);
				if (sqlDataReader.HasRows)
				{
					processReadResultsFunc.Invoke(sqlDataReader);
				}

				sqlDataReader.Close();
			}
		}
	}
}
