<Query Kind="Program">
  <Output>DataGrids</Output>
</Query>

void Main()
{
	var invoiceNumbers = new[] { "A0001", "A002", "A0031"};
	var certParams = GetSqlParameters(out string certParamsList, SqlDbType.VarChar, invoiceNumbers);
                var queryResult = Database.QueryValue(
                    $"SELECT MIN([EndManufacturerDate]) FROM [dbo].[Equipment] WHERE CertificateNumber IN ({certParamsList})",
                    certParams);

}

public static string GetSqlParametersList(params SqlParameter[] parameters)
{
	return string.Join(", ", parameters.Select(p => $"@{p.ParameterName}"));
}

public static SqlParameter[] GetSqlParameters<T>(out string sqlParamsList, SqlDbType paramType, params T[] values)
{
	return GetSqlParameters(out sqlParamsList, values.Select(v => Tuple.Create(paramType, (object)v)).ToArray());
}

public static SqlParameter[] GetSqlParameters(out string sqlParamsList, params Tuple<SqlDbType, object>[] values)
{
	var parameters = values
		.Select((value, i) => new SqlParameter($"Param{i}", value.Item1) { Value = value.Item2 })
		.ToArray();

	sqlParamsList = GetSqlParametersList(parameters);
	return parameters;
}