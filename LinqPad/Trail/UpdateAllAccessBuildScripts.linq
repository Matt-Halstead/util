<Query Kind="Program" />

void Main()
{
    var rootPath = @"E:\dev\DevOps\AccessApps\source";

    var buildFiles= Directory.GetFiles(rootPath, "*.bat", SearchOption.AllDirectories)
        .Select(path => new FileInfo(path))
        .Where(info => info.Directory.Name == ".build")
        .ToArray();

    foreach (var buildFile in buildFiles)
    {
        var buildFolder = buildFile.Directory;
        var productFolder = buildFolder.Parent;
        var areaFolder = productFolder.Parent;
        
        var content = File.ReadAllText(buildFile.ToString());

        var replaced = content
            .Replace("SET APPFOLDER=DELIVERY", $"SET APPFOLDER={areaFolder}")
            .Replace("SET APPNAME=DelAdmin", $"SET APPNAME={productFolder}");

        File.WriteAllText(buildFile.ToString(), replaced);
        Console.WriteLine(buildFile.ToString());
    }
}

