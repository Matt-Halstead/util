<Query Kind="Program" />

void Main()
{
    var pathExceptions = new[] {"\\.build", "\\CustomCommandBars", "\\ExternalFiles", "\\Scripts", "\\Indexes"};
    // All the files to be searched
    var files = Directory.EnumerateFiles(@"E:\dev\DevOps\AccessApps\source", "*.*", SearchOption.AllDirectories)
        .Where(path => !pathExceptions.Any(pe => path.Contains(pe)))
        .ToArray();

    // All the target strings to be searched for
    //var delIssueFiles = Directory.EnumerateFiles(@"E:\dev\Archive\ArchivedAccessApps\DELIVERY\DelIssue\Queries");
    //var invIssueFiles = Directory.EnumerateFiles(@"E:\dev\Archive\ArchivedAccessApps\IT\InvIssue\Queries");
    var builDelFiles = Directory.EnumerateFiles(@"E:\dev\Archive\ArchivedAccessApps\BUILDER\BuilDel\Queries");
//    var searchTargets = builDelFiles
//        .Select(f => Path.GetFileNameWithoutExtension(f))
//        .Distinct()
//        .ToArray();
    
    var searchTargets = new[] { "qprBuilD_DateSpCust" };
   
   
    var hitsByTarget = new Dictionary<string, List<string>>();
    var hitsByFile = new Dictionary<string, List<string>>();
    
    foreach (var file in files)
    {
        if (!hitsByFile.TryGetValue(file, out _))
        {
            hitsByFile[file] = new List<string>();
        }
        
        SetPreambleLine($"\n{file}");

        var lines = File.ReadAllLines(file);        
        int lineNumber = 1;
        
        foreach (var line in lines)
        {
            foreach (var target in searchTargets)
            {
                if (!hitsByTarget.TryGetValue(target, out _))
                {
                    hitsByTarget[target] = new List<string>();
                }
    
                if (line.ToLower().Contains(target.ToLower()))
                {
                    var result = $"  [{lineNumber}] - Matched '{target}': {line}";
                    WriteLine(result);

                    hitsByTarget[target].Add(result);
                    hitsByFile[file].Add(result);
                }
            }
            
            lineNumber++;
        }
    }
    
    preamble.Clear();

    WriteLine("\nTargets not found in any file:");
    hitsByTarget.Where(pair => pair.Value.Count == 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

//    WriteLine("\nFiles containing no targets:");
//    hitsByFile.Where(pair => pair.Value.Count == 0)
//        .Select(pair => pair.Key)
//        .ToList()
//        .ForEach(key => WriteLine($"    {key}"));

    WriteLine("\nTargets found in at least one file:");
    hitsByTarget.Where(pair => pair.Value.Count > 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

    WriteLine("\nFiles containing at least one target:");
    hitsByFile.Where(pair => pair.Value.Count > 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

    Console.WriteLine("");
    Console.WriteLine("Done.");
}

// This will only be written out once, before the next call to WriteLine().
private static List<string> preamble = new List<string>();

private static void SetPreambleLine(string line)
{
    if (!preamble.Contains(line))
    {
        preamble.Clear();
        preamble.Add(line);
    }
}

private static string ToTitleLine(string line)
{
    return $"{line}\n{new String('-', line.Length)}";
}

private static void WriteLine(string line)
{
    preamble.ForEach(p => Console.WriteLine(ToTitleLine(p)));
    preamble.Clear();
    Console.WriteLine(line);
}