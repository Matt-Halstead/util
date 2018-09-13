<Query Kind="Program" />

void Main()
{
	var folder = @"C:\temp\TrackerImageStore";	
	var files = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly).ToArray();
	
	foreach (var file in files)
	{
		var f = file.Replace("20171123-", "");
		f = f.Replace("165849_", "165850-");
		
		var before = Path.Combine(folder, file);
		var after = Path.Combine(folder, f);
		
		Console.WriteLine($"Renamed {before} to {after}.");
		
		File.Move(Path.Combine(folder, file), Path.Combine(folder, f));
	}
}
