<Query Kind="Program" />

void Main()
{
    // All the files to be searched
    var files = Directory.EnumerateFiles(@"E:\dev\DevOps\AccessApps\source\DELIVERY\DelMan", "*.*", SearchOption.AllDirectories)
        .ToArray();

    // All the target strings to be searched for
    var reportFiles = Directory.EnumerateFiles(@"E:\dev\DevOps\AccessApps\source\DELIVERY\DelMan\Reports");
    var searchTargets = reportFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
    
    // var searchTargets = new[] { "one", "two" };
   
   
    var hitsByTarget = new Dictionary<string, List<string>>();
    var hitsByFile = new Dictionary<string, List<string>>();
    
    foreach (var file in files)
    {
        if (!hitsByFile.TryGetValue(file, out _))
        {
            hitsByFile[file] = new List<string>();
        }
        
        SetPreambleLine($"\n{Path.GetFileName(file)}");

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

    SetPreambleLine("\nTargets not found in any file:");
    hitsByTarget.Where(pair => pair.Value.Count == 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

    SetPreambleLine("\nFiles containing no targets:");
    hitsByFile.Where(pair => pair.Value.Count == 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

    SetPreambleLine("\nTargets found in at least one file:");
    hitsByTarget.Where(pair => pair.Value.Count > 0)
        .Select(pair => pair.Key)
        .ToList()
        .ForEach(key => WriteLine($"    {key}"));

    SetPreambleLine("\nFiles containing at least one target:");
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
        preamble.Add(new String(Enumerable.Repeat('-', line.Length).ToArray()));
    }
}

private static void WriteLine(string line)
{
    preamble.ForEach(p => Console.WriteLine(p));
    preamble.Clear();
    Console.WriteLine(line);
}