<Query Kind="Program">
  <Output>DataGrids</Output>
  <NuGetReference>Dapper</NuGetReference>
  <Namespace>Dapper</Namespace>
</Query>

void Main()
{
	
	using (IDbConnection central = new SqlConnection("Data Source=AppSQL;Integrated Security=True;Application Name=DapperTest"))
	{
		// Returns a single dynamic row object, with ftxBranch = 00
//		var value = central.ExecuteScalar("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = '00'");
//value.Dump();

//		var sqlParams = new DynamicParameters();
//		sqlParams.Add("@fidUser", 1);
//		sqlParams.Add("@fidRegion", 2);
//		sqlParams.Add("@fdtTrip", DateTime.Now);
//
//		var sql = $"Insert into tableName ({string.Join(", ", sqlParams.ParameterNames)}) VALUES ({string.Join(", ", from p in sqlParams.ParameterNames select $"@{p}")});";
//		
//		sql.Dump();

try
{
			var result = central.QueryFirst("Select [ftxBranch] From [Central].[dbo].[tblBranch]").SafeGetOne<dynamic, string>("");
var foo = result;
		}
catch (Exception e)
{
	e.Dump();
}
		//		// Returns a collection of many dynamic objects, one for each branch
//		var manyRows = central.Query("Select [ftxBranch] From [Central].[dbo].[tblBranch]");		
//
//		// Returns an empty collection, as no branches match
//		var noRows = central.Query("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] Like '%xxxxx%'");
//
//		// Returns a single dynamic row object, with ftxBranch = 00
//		var singleRow = central.QuerySingle("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = '00'");
//
//		// Returns null row, no matching row found
//		var singleNullRow = central.QuerySingleOrDefault("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = 'x'");
//
//		// Throws exception, result contains no rows.
//		//var singleRowEx1 = central.QuerySingle("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = 'x'");
//
//		// Throws exception, result contains >1 rows.
//		//var singleRowEx2 = central.QuerySingle("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] Like '%3%'");
//
//		// Returns a single dynamic object, the first where ftxBranch = 00
//		var firstRow1 = central.QueryFirst("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = '00'");
//
//		// Returns a single dynamic object, the first where ftxBranch starts with or ends with 3
//		var firstRow2 = central.QueryFirst("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] Like '%3%'");
//
//		// Returns null row, no matching row found
//		var firstNullRow = central.QueryFirstOrDefault("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = 'x'");

		// Throws exception, result contains no rows.
		//var firstResultEx = central.QueryFirst("Select [ftxBranch] From [Central].[dbo].[tblBranch] Where [ftxBranch] = 'x'");
	}
	

}

public static class Extensions
{
	public static bool TryGetOne<Tq, Tr>(this Tq queryResult, out Tr oneValue)
	{
		oneValue = default(Tr);

		object rawValue = null;

		if (queryResult != null)
		{
			if (queryResult is IEnumerable<KeyValuePair<string, object>> pairs)
			{
				var first = pairs.FirstOrDefault();
				rawValue = first.Value;
			}
			else
			{
				rawValue = queryResult;
			}
		}

		return TryCast<Tr>(rawValue, out oneValue, false);
	}

	public static Tr SafeGetOne<Tq, Tr>(this Tq queryResult, Tr defaultValue)
	{
		return TryGetOne<Tq, Tr>(queryResult, out Tr oneValue) ? oneValue : defaultValue;
	}

	public static bool TryCast<T>(this object obj, out T result, bool strict = true)
	{
		result = default(T);
		if (obj is T)
		{
			result = (T)obj;
			return true;
		}

		// If it's null, we can't get the type.
		if (obj != null)
		{
			try
			{
				var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
				if (converter.CanConvertFrom(obj.GetType()))
				{
					result = (T)converter.ConvertFrom(obj);
				}
				else
				{
					converter = System.ComponentModel.TypeDescriptor.GetConverter(obj.GetType());
					if (converter.CanConvertTo(typeof(T)))
					{
						result = (T)converter.ConvertTo(obj, typeof(T));
					}
					else
					{
						return false;
					}
				}
			}
			catch (Exception)
			{
				if (strict)
				{
					throw;
				}
				return false;
			}

			return true;
		}

		//Be permissive if the object was null and the target is a ref-type
		return !typeof(T).IsValueType;
	}
}
