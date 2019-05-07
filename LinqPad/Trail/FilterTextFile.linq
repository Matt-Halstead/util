<Query Kind="Program" />

void Main()
{
    const string InputFilename = @"C:\temp\t_drive_listing.txt";
    
    var linesWithNumbers = File.ReadAllLines(InputFilename)
        .Select((line, i) => Tuple.Create(line, i+1))
        .ToArray();
    
    var excludeFilters = new Func<Tuple<string, int>, bool>[] {
        t => getFileName(t.Item1) == ".ssh",
    };

    var includedExtensions = new[] { ".exe", ".mdb", ".accdb"};

    var includeFilters = new Func<Tuple<string, int>, bool>[] {
        t => includedExtensions.Contains(getExt(t.Item1))
    };

    var filteredLines = linesWithNumbers
        .Where(t => includeFilters.Any(filter => filter(t)))
        .Where(t => !excludeFilters.Any(filter => filter(t)))
        .ToArray();
    
    var tempLines = filteredLines.Select(t => t.Item1).ToArray();
    
    filteredLines = filteredLines
        .Where(t => !hasExistingSubString(t.Item1, tempLines))
        .ToArray();
        
    filteredLines.Dump();
    
//    File.WriteAllLines(
//        @"c:\temp\filtered_list.txt",
//        filteredLines.Select(t => t.Item1).ToArray());
}

private string getFileName(string line) => Path.GetFileName(line);

private string getFileNameNoExt(string line) => Path.GetFileNameWithoutExtension(line);

private string getExt(string line) => Path.GetExtension(line);



private bool hasExistingSubString(string subject, string[] others)
{
    return false;
    
    
    
    var shortSubject = getFileNameNoExt(subject);

    var otherFiles = others.Where(path => getExt(path) != "" || !Directory.Exists(path)).ToArray();
    
    return otherFiles.Any(other =>
    {
        var shortOther = getFileNameNoExt(other);
        
        if (shortOther != shortSubject &&
            shortOther.Contains(".") &&
            shortSubject.StartsWith(shortOther))
        {
            //Console.WriteLine($"Substring path '{shortOther}' already defined, skipped: {shortSubject}");
            return true;
        }
        
        return false;
    });
}