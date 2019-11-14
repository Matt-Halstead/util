<Query Kind="Program">
  <Connection>
    <ID>96aebd64-c7ff-4293-b35c-0fd424059f68</ID>
    <Persist>true</Persist>
    <Server>appsql</Server>
    <Database>ComercoEW</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

// Reads xml files, changes their content, saves them again.

void Main()
{
    // get the files to be processed
    var files = new[] {
        @"\\appsql\ComercoEWXML\Archive\trail_20190808_002.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190808_003.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190808_004.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190809_001.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190809_002.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190812_002.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190812_003.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190812_004.xml",
        @"\\appsql\ComercoEWXML\Archive\trail_20190812_005.xml"
    };

    // get invalid certificates
    var startDate = "2019-08-08";
    var invalidCertsList = ExecuteQueryDynamic(
$@"SELECT [CertificateNumber], [Error]
FROM [ComercoEW].[dbo].[ContractXML]
where LastUpdatedDate > '{startDate}' And Error <> '';")
        .Select(row => Tuple.Create<string, string>(row[0], row[1]))
        .OfType<Tuple<string, string>>()
        .ToArray();

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
        var retailerNode = xml.Element("Retailer");
        if (retailerNode == null)
        {
            throw new InvalidOperationException("Xml format invalid!");
        }

        string before = retailerNode.ToString();

        foreach (var node in FindInvalidCerts(retailerNode, invalidCertsList))
        {
            node.Remove();
        }

        string after = retailerNode.ToString();

        if (before != after)
        {
            // save files
            //this.BackupFile(file);
            this.SaveCopyOfFile(file, xml);

            string.Format("Info, file '{0}' was transformed.", file).Dump();
        }
        else
        {
            string.Format("Info, no changes made to file '{0}'.", file).Dump();
        }
    }

    string.Format("Info, done.").Dump();

}

//void BackupFile(string filename)
//{
//    if (File.Exists(filename))
//    {
//        var bakFilename = string.Format("{0}.orig", filename);
//        if (!File.Exists(bakFilename))
//        {
//            File.Copy(filename, bakFilename, false);
//            string.Format("Info, backup file '{0}' created ok.", bakFilename).Dump();
//        }
//        else
//        {
//            //string.Format("Info, backup file '{0}' already exists, not backed up.", bakFilename).Dump();
//        }
//    }
//    else
//    {
//        string.Format("Error, cannot backup file '{0}', does not exist.", filename).Dump();
//        throw new InvalidOperationException("Backup failed.");
//    }
//}

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

IEnumerable<XElement> FindInvalidCerts(XElement x, Tuple<string, string>[] invalidCerts)
{
    var customerNodes = x.Descendants().Where(d => d.Name.LocalName == "Customer").ToArray();
    foreach (var c in customerNodes)
    {
        XElement contract = null;
        XElement cert = null;
        if ((contract = c.Element("Contract")) != null)
        {
            if ((cert = contract.Element("CertificateNumber")) != null)
            {
                var invalid = invalidCerts.FirstOrDefault(t => t.Item1.Equals(cert.Value));
                if (invalid != null)
                {
                    Console.WriteLine($"! Customer {c.Element("LastName").Value} has invalid contract, certificate number {cert.Value}.  Error:\n   => {invalid.Item2}");
                    yield return c;
                }
                else
                {
                    Console.WriteLine($"Customer {c.Element("LastName").Value} has valid contract, certificate number {cert.Value}.");    
                }
            }
        }
    }
}

void SaveCopyOfFile(string originalFilename, XElement x)
{
    const string OutputPath = @"\\appsql\ComercoEWXML\temp";
    try
    {
        var baseFilename = Path.GetFileName(originalFilename);
        var newFilename = Path.Combine(OutputPath, baseFilename);
        if (newFilename.Equals(originalFilename, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new InvalidOperationException($"Cannot overwrite file: {originalFilename}");
        }
        
        x.Document.Save(newFilename);
        Console.WriteLine($"Wrote updated file to: {newFilename}");
    }
    catch
    {
        string.Format("Error, could not save file '{0}'.", originalFilename).Dump();
        throw new InvalidOperationException("Save failed");
    }
}

// ===================================================================
class DynamicDataRecord : System.Dynamic.DynamicObject
{
    readonly IDataRecord _row;
    public DynamicDataRecord(IDataRecord row) { _row = row; }

    public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
    {
        if (binder.Type == typeof(IDataRecord))
        {
            result = _row;
            return true;
        }
        return base.TryConvert(binder, out result);
    }

    public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
    {
        if (binder.Name == "Dump")
        {
            if (args.Length == 0)
                _row.Dump();
            else if (args.Length == 1 && args[0] is int)
                _row.Dump((int)args[0]);
            else if (args.Length == 1 && args[0] is string)
                _row.Dump((string)args[0]);
            else if (args.Length == 2)
                _row.Dump(args[0] as string, args[1] as int?);
            else
                _row.Dump();
            result = _row;
            return true;
        }
        return base.TryInvokeMember(binder, args, out result);
    }

    public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
    {
        result = _row[binder.Name];
        if (result is DBNull) result = null;
        return true;
    }

    public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
    {
        if (indexes.Length == 1)
        {
            result = _row[int.Parse(indexes[0].ToString())];
            return true;
        }
        return base.TryGetIndex(binder, indexes, out result);
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return Enumerable.Range(0, _row.FieldCount).Select(i => _row.GetName(i));
    }
}