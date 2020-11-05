<Query Kind="Program">
  <Output>DataGrids</Output>
  <Namespace>Microsoft.Win32</Namespace>
</Query>

void Main()
{
	EnumDsn(Registry.CurrentUser).Dump("CurrentUser");
	EnumDsn(Registry.LocalMachine).Dump("LocalMachine");
}

class DSNInfo
{
	public string Name { get; set; } = "";
	public string DriverDescription { get; set; } = "";
	public string Driver { get; set; } = "";
	public string Description { get; set; } = "";
	public string Server { get; set; } = "";
	public string Database { get; set; } = "";
	public string LastUser { get; set; } = "";
	public string Trusted_Connection { get; set; } = "";
}

private IEnumerable<DSNInfo> EnumDsn(RegistryKey rootKey)
{
	var dsnList = new List<DSNInfo>();
	
    RegistryKey dsnRootRegKey = rootKey.OpenSubKey(@"SOFTWARE\Wow6432Node\ODBC\ODBC.INI");
	
    RegistryKey dsnListRegKey = dsnRootRegKey?.OpenSubKey("ODBC Data Sources");
	if (dsnListRegKey != null)
	{
		foreach (string dsnName in dsnListRegKey.GetValueNames())
		{
			string dsnDriverDescription = dsnListRegKey.GetValue(dsnName, "").ToString();
            
            var dsnRegKey = dsnRootRegKey.OpenSubKey(dsnName);
            if (dsnRegKey != null)
			{
				dsnList.Add(new DSNInfo
				{
                    Name = dsnName,
					DriverDescription = dsnDriverDescription,
					Driver = dsnRegKey.GetValue("Driver", "").ToString(),
					Description = dsnRegKey.GetValue("Description", "").ToString(),
					Server = dsnRegKey.GetValue("Server", "").ToString(),
					Database = dsnRegKey.GetValue("Database", "").ToString(),
					LastUser = dsnRegKey.GetValue("LastUser", "").ToString(),
					Trusted_Connection = dsnRegKey.GetValue("Trusted_Connection", "").ToString()
				});
            }
		}
	}
	
	return dsnList;
}