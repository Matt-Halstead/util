<Query Kind="Program" />

void Main()
{
    var subject = File.ReadAllText(@"E:\work\AutoMate 10\Tasks\10J - Misc. Syspro Reports (monthly).aml");
    
    foreach (var pair in FromToPairs)
    {
        var result = ReplaceAll(subject, pair.Key, pair.Value);
        subject = result;
    }
    
    subject.Dump("Result");

    FromToPairs.Select(p => $"<AMVARIABLE NAME=\"{p.Value}\" VALUE=\"{p.Key}\" />").Dump("var defs");
}

private static readonly Dictionary<string, string> FromToPairs = new Dictionary<string, string> {
{ @"%var_Path_SysproReports%\%var_DateWeekday% ARDC-ME.pdf", @"var_Report_ARDC-ME" },
{ @"%var_Path_SysproReports%\%var_DateWeekday% ARBR-TYPE.pdf", @"var_Report_ARBR-TYPE" },
{ @"%var_Path_SysproReports%\%var_DateWeekday% HEALTHYTOT.pdf", @"var_Report_HEALTHYTOT" },
{ @"%var_Path_SysproReports%\%var_DateWeekday% ARDC-CL.pdf", @"var_Report_ARDC-CL" }
};

private static string ReplaceAll(string subject, string find, string replace)
{
    if (string.IsNullOrEmpty(subject))
    {
        return subject;
    }

    Console.WriteLine($"<AMVARIABLE NAME=\"{replace}\" VALUE=\"{find}\" />");

    return subject.Replace(find, $"%{replace}%");
}