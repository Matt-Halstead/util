<Query Kind="Program" />

// Reads xml files, changes their content, saves them again.

void Main()
{
    // get the files to be processed
    var files = this.GetFilenamesToEdit();
    
    foreach (var file in files)
    {
        // read xml
        XElement xml = this.ReadFile(file);
        if (xml == null)
        {
            string.Format("Error, cannot read file '{0}'.", file).Dump();
            return;
        }

        // process them
        string before = xml.ToString();
        var xmlNew = this.StripOutAssemblySigningNodes(xml);
        string after = xmlNew.ToString();

        if (before != after)
        {
            // save files
            //this.BackupFile(file);
            this.SaveFile(file, xmlNew);
            
            string.Format("Info, file '{0}' was transformed.", file).Dump();
        }
        else
        {
            string.Format("Info, no changes made to file '{0}'.", file).Dump();
        }
    }

    string.Format("Info, done.").Dump();

}

string[] GetFilenamesToEdit()
{
    var rootPaths = new[]
    {
        @"C:\work\source\MSched.Development\Development\Dev\Products",
        @"C:\work\source\MSched.Development\Development\Dev\Libraries"
    };
    
    var fileList = new List<string>();
    
    foreach (var rootPath in rootPaths)
    {
        if (Directory.Exists(rootPath))
        {
            var projFiles = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);
            fileList.AddRange(projFiles);
        }
        else
        {
            string.Format("Warning, root path '{0}' does not exist.", rootPath).Dump();
        }
    }
    
    return fileList.ToArray();
}

void BackupFile(string filename)
{
    if (File.Exists(filename))
    {
        var bakFilename = string.Format("{0}.orig", filename);
        if (!File.Exists(bakFilename))
        {
            File.Copy(filename, bakFilename, false);
            string.Format("Info, backup file '{0}' created ok.", bakFilename).Dump();
        }
        else
        {
            //string.Format("Info, backup file '{0}' already exists, not backed up.", bakFilename).Dump();
        }
    }
    else
    {
        string.Format("Error, cannot backup file '{0}', does not exist.", filename).Dump();
        throw new InvalidOperationException("Backup failed.");
    }
}

XElement ReadFile(string filename)
{
    XElement xml = null;
    try
    {
        XDocument doc = XDocument.Parse(File.ReadAllText(filename, Encoding.UTF8));
        xml = doc != null ? doc.Elements().FirstOrDefault() : null;
    }
    catch
    {
        xml = null;
    }

    return xml;
}

XElement StripOutAssemblySigningNodes(XElement x)
{
    var nodesToRemove = new[]
    {
        x.Descendants().FirstOrDefault(d => d.Name.LocalName == "AssemblyOriginatorKeyFile"),
        x.Descendants().FirstOrDefault(d => d.Name.LocalName == "SignAssembly")
    }.Where(n => n != null).ToArray();
    
    foreach (var node in nodesToRemove)
    {
        var parent = node.Parent;
        node.Remove();
        
        if (parent.Elements().Count() == 0)
        {
            parent.Remove();
        }        
    }
    
    return x;
}

void SaveFile(string filename, XElement x)
{
    try
    {
        x.Document.Save(filename);
    }
    catch
    {
        string.Format("Error, could not save file '{0}'.", filename).Dump();
        throw new InvalidOperationException("Save failed");
    }
}

