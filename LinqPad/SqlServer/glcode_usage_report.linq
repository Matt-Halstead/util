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
	Program.Go();
}

class Program
{
	public static void Go()
	{
		var sysproGlCodeSummary = new List<dynamic>();
		var dwAccountActivitySummaryQuery = new List<dynamic>();

		try
		{
			using (var trailSql = new SqlConnection(@"Data Source=TRAILSQL;Initial Catalog=SysproCompanyA;Integrated Security=True;Application Name=Matt's LinqPad Query"))
			{
				// Query to get summary info for all defined GL codes

				var sysproGlCodesSql = @"
SELECT [GlCode]
      ,[Description]
      ,[AccountType]
      ,[ExpenseType]
      ,[PtdDrValue]
      ,[PtdCrValue]
      ,[CurrentBalance]
      ,[PrevPerEndBal]
      ,[GlGroup]
      ,[BudgetMethod]
      ,[ControlAccFlag]
      ,[AccOnHldFlag]
      ,[TaxCode]
      ,[RetEarnsAccount]
      ,[TaxAccount]
      ,[InterestAccount]
FROM [SysproCompanyA].[dbo].[GenMaster]
WHERE [Company] = 'A';";
				sysproGlCodeSummary = trailSql.ExecSql(sysproGlCodesSql)
					.ToList();
			}

			using (var onpremdwSql = new SqlConnection(@"Data Source=ONPREMDWSQL\DW;Initial Catalog=MiningOLAP;Integrated Security=True;Application Name=Matt's LinqPad Query"))
			{
				// Query to get account codes that have been used ever.
				string dwAccountActivitySummarySql = $@"
SELECT [Account]
      ,MAX([Company]) as [Company]
      ,MAX([TransactionDate]) as [MaxTransactionDate]
FROM [MiningOLAP].[dbo].[vwm_TrailSAWGL_JournalModel]
WHERE [Company] = 'A'
GROUP BY [Account];
	";
				dwAccountActivitySummaryQuery = onpremdwSql.ExecSql(dwAccountActivitySummarySql)
					.ToList();
			}
		}
		catch (Exception e)
		{
			$"Error while executing query - {e}".Dump(e.Message);
			return;
		}

		try
		{
			// Absolutely all currently defined Syspro codes.
			var sysproGlCodeNames = sysproGlCodeSummary
				.Select(row => row.GlCode);

			// Get journal accounts that are known in Syspro, note those that are orphaned.
			var allTransactionAccountNames = dwAccountActivitySummaryQuery
				.Select(row => row.Account.Trim())
				.ToList();

			var orphanTransactionAccountNames = allTransactionAccountNames
				.Except(sysproGlCodeNames);

			var dwAccountCodeNames = allTransactionAccountNames
				.Except(orphanTransactionAccountNames)
				.ToList();

			// Find active and inactive journal accounts.
			var fiveYearsAgo = DateTime.Now.AddYears(-5);
			var dwActiveAccounts = dwAccountActivitySummaryQuery
				.Where(row => row.MaxTransactionDate >= fiveYearsAgo)
				.ToList();

			var dwInactiveAccounts = dwAccountActivitySummaryQuery
				.Except(dwActiveAccounts)
				.ToList();

			// Find the syspro GL codes corresponding to the unused and inactive journal accounts, for removal.
			var sysproCodeNamesConsideredForDeletion = sysproGlCodeSummary
				.Where(row => row.ControlAccFlag == "N")
				.Select(row => row.GlCode);

			var sysproGlCodeNamesAbsentFromDw = sysproCodeNamesConsideredForDeletion
				.Except(dwAccountCodeNames)
				.ToList();

			var sysproInactiveAccountNames = sysproCodeNamesConsideredForDeletion
				.Except(sysproGlCodeNamesAbsentFromDw)
				.Intersect(dwInactiveAccounts.Select(row => row.Account.Trim()))
				.ToList();

			// Dump out summary of what to remove.
			sysproGlCodeSummary
				.Where(row => row.ControlAccFlag == "Y")
				.Dump($"These Syspro CONTROL GL Codes will not be removed:");

			sysproGlCodeSummary
				.Where(row => sysproGlCodeNamesAbsentFromDw.Contains(row.GlCode))
				.Dump($"REMOVE Syspro GL Codes NEVER USED:");

			sysproGlCodeSummary
				.Where(row => sysproInactiveAccountNames.Contains(row.GlCode))
				.Dump($"REMOVE Syspro GL Codes NOT USED IN 5 YEARS:");

			// Note any inconsistencies
			orphanTransactionAccountNames
				.Dump($"DW Transaction Accounts unknown in Syspro and ignored:");
		}
		catch (Exception e)
		{
			$"Error while processing query results - {e}".Dump(e.Message);
		}

		// TODO, dump to csv files?
		//
		//				var csv = ToCSV(
		//					"AppName, ConnectDB, DBFullName",
		//					fooRows,
		//					r => new string[] { r.AppName, r.ConnectDB, r.DBFullName });
		//				csv.Dump();
	}

	private static string ToCSV<TRow>(string header, IEnumerable<TRow> rows, Func<TRow, string[]> toLineFunc)
	{
		var sb = new StringBuilder();

		sb.AppendLine(header);

		foreach (var row in rows)
		{
			var line = string.Join(", ", toLineFunc(row).Select(CleanupCellValue));
			sb.AppendLine(line);
		}

		return sb.ToString();
	}

	private static string CleanupCellValue(string cell)
	{
		var value = cell?.Trim() ?? string.Empty;
		if (value.Contains(','))
		{
			value = $"\"{value}\"";
		}

		return value;
	}
}

static class DatabaseExtensions
{
	// Create a connection, create a command from given sql, execute the given execCommandAction.
	// SqlDataReader is created from command resultes, which is cast to IDataRecord and wrapped in DynamicDataRecord helper
	// for access to fields via 'dynamic'.
	//
	// NOTE: You dont need to use this if you are dealing with only a single DB, or multiples from the same server.
	// In that case you can use the built-in 'ExecuteQueryDynamic()' method.
	public static IEnumerable<dynamic> ExecSql(this SqlConnection connection, string sql)
	{
		$"Running sql command ...\n{sql}".Dump();
		
		using (var command = new SqlCommand(sql, connection) { CommandTimeout = 60 }) // timeout = 1 minute
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
			$"\nSql command complete.\n".Dump();

			return reader
				.Cast<IDataRecord>()
				.Select(r => new DynamicDataRecord(r));
		}
	}
}

class DynamicDataRecord : System.Dynamic.DynamicObject
{
	readonly IDataRecord _row;
	public DynamicDataRecord(IDataRecord row) { _row = row; }

	public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
	{
		if (binder.Type == typeof(IDataRecord))
		{
			result = _row;
			return true;
		}
		return base.TryConvert(binder, out result);
	}

	public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
	{
		if (binder.Name == "Dump")
		{
			if (args.Length == 0)
				_row.Dump();
			else if (args.Length == 1 && args[0] is int)
				_row.Dump((int)args[0]);
			else if (args.Length == 1 && args[0] is string)
				_row.Dump((string)args[0]);
			else if (args.Length == 2)
				_row.Dump(args[0] as string, args[1] as int?);
			else
				_row.Dump();
			result = _row;
			return true;
		}
		return base.TryInvokeMember(binder, args, out result);
	}

	public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
	{
		result = _row[binder.Name];
		if (result is DBNull) result = null;
		return true;
	}

	public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
	{
		if (indexes.Length == 1)
		{
			result = _row[int.Parse(indexes[0].ToString())];
			return true;
		}
		return base.TryGetIndex(binder, indexes, out result);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		return Enumerable.Range(0, _row.FieldCount).Select(i => _row.GetName(i));
	}
}
