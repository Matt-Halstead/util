<Query Kind="Program" />

void Main()
{
	var root = @"C:\Source\Seymour\Main Branch";
	var paths = Directory.GetDirectories(root)
		.Where(path => Directory.Exists(path)).ToList();
	
	var result = paths.Except(keepList.Select(path => Path.Combine(root, path)));
	
	result.Dump("results");
}

static string[] keepList = new string[]
{
	".nuget",
	"Common"
};