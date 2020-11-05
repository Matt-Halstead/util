<Query Kind="Program" />

void Main()
{
    var filenameToTaskNameLookup = GetFilenameToTaskNameLookup();
    
    var taskFileNames = Directory.GetFiles(@"E:\work\AutoMate 10\Tasks", "*.aml").ToArray();

    foreach (var taskFile in taskFileNames)
    {
        var xml = File.ReadAllText(taskFile);
        var basicFilename = Path.GetFileName(taskFile);

        if (filenameToTaskNameLookup.TryGetValue(basicFilename, out string taskName))
        {
            if (UpdateTaskVarInXml(xml, taskName, out string updatedXml))
            {
                updatedXml.Dump();
                File.WriteAllText(taskFile, updatedXml);
            }
        }
    }    
}

private static Dictionary<string, string> GetFilenameToTaskNameLookup()
{
    Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    
    var taskFile = @"E:\work\AutoMate 10\AutoMate10TaskFile.atl";
    
    var doc = XDocument.Parse(File.ReadAllText(taskFile));
    var tuples = doc
        ?.Element("AMTASKFILE")
        ?.Descendants("TASK")
        ?.Select(x => Tuple.Create(x.Attribute("FILENAME").Value, x.Attribute("NAME").Value))
        ?? Enumerable.Empty<Tuple<string, string>>();
        
    foreach (var tuple in tuples)
    {
        var basicFilename = Path.GetFileName(tuple.Item1);
        result[basicFilename] = tuple.Item2;
    }    
    
    return result;
}

private static bool UpdateTaskVarInXml(string xml, string taskName, out string updatedXml)
{
    updatedXml = xml;
    
    var doc = XDocument.Parse(xml);

    var varElements = doc
        ?.Element("AMTASK")
        ?.Element("AMFUNCTION")
        ?.Elements("AMVARIABLE")
        ?? Enumerable.Empty<XElement>();
        
    foreach (var x in varElements)
    {
        var nameAttr = x.Attribute("NAME");
        var valueAttr = x.Attribute("VALUE");
        if (nameAttr != null && valueAttr != null && nameAttr.Value.Equals("var_Task"))
        {
            valueAttr.Value = taskName;
            updatedXml = doc.Root.ToString();
            return true;
        }
    }

    return false;
}
