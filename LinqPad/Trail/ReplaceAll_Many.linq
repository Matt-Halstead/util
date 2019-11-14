<Query Kind="Program" />

void Main()
{
    var subject = LoadSubject();

    // key is the replacement target, value is the list of aliases to be replaced
    var aliasesToTargetMap = LoadReplacements();
    
    // 1st is the element to replace, 2nd is the replacement.
    var targetsToAliases = new List<Tuple<string, string>>();
    foreach (var pair in aliasesToTargetMap)
    {
        pair.Value.ToList().ForEach(l => targetsToAliases.Add(Tuple.Create(l, pair.Key)));
    }
    
    var original = subject.ToString();
    
    foreach (var pair in targetsToAliases)
    {
        var result = ReplaceAll(subject, $"{pair.Item1};", $"{pair.Item2};");
        subject = result;
    }
    
    original.Dump("Original");
    subject.Dump("Result");
}

private static string ReplaceAll(string subject, string find, string replace)
{
    var result = Replace(subject, find, replace, StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"IN : {subject}");
    Console.WriteLine($"OUT: {result}");
    return result;
}

private static string LoadSubject()
{
    //return File.ReadAllText(@"E:\work\AutoMate 10\Tasks\10J - Misc. Syspro Reports (monthly).aml");

    return @"UPDATE Multi.dbo.tblGlobalMm
SET
	ftxEmailWhenNewProj = src.ftxEmailWhenNewProj,
	ftxOutsProjValueEmail = src.ftxOutsProjValueEmail,
	ftxNearCompEmail = src.ftxNearCompEmail,
	ftxIrPurchEmail = src.ftxIrPurchEmail,
	ftxEmailWhenPvfAppr = src.ftxEmailWhenPvfAppr,
	ftxSalesMarginEmail = src.ftxSalesMarginEmail,
	ftxEmailMultiDeli = src.ftxEmailMultiDeli,
	ftxMMNorthMgrEmail = src.ftxMMNorthMgrEmail,
	ftxMMSouthMgrEmail = src.ftxMMSouthMgrEmail
FROM
  Multi.dbo.tblGlobalMm AS multi
  INNER JOIN
  (
    VALUES
    (1
	,'glens@trailappliances.com;jamesr@trailappliances.com;jasonb@trailappliances.com;justine@trailappliances.com; laudv@trailappliances.com'
	,'jasonb@trailappliances.com; jamesr@trailappliances.com; rhamilton@trailappliances.com; CreditDept@trailappliances.com; rbroderick@trailappliances.com; kvanvlack@trailappliances.com; lvidal@trailappliances.com; Larry Law; jerikson@trailappliances.com'
	,'rbroderick@trailappliances.com; kvanvlack@trailappliances.com; CreditDept@trailappliances.com; Wendy Uyeda'
	,'stephaniek@trailappliances.com'
	,'richardb@trailappliances.com;skrzyz@trailappliances.com; justine@trailappliances.com;kvanvlack@trailappliances.com;parisaz@trailappliances.com; laurawilson@trailappliances.com'
	,'jasonb@trailappliances.com; jamesr@trailappliances.com; Lvidal@trailappliances.com; rhamilton@trailappliances.com; jerikson@trailappliances.com'
	,'mfprojectcoordinators@trailappliances.com; sfbuilderadmin@trailappliances.com'
	,'lvidal@trailappliances.com;jerikson@trailappliances.com'
	,'gsharpe@trailappliances.com;jerikson@trailappliances.com;lvidal@trailappliances.com'
	)
  )
  AS src (fIdGlobal, ftxEmailWhenNewProj, ftxOutsProjValueEmail, ftxNearCompEmail, ftxIrPurchEmail, ftxEmailWhenPvfAppr, ftxSalesMarginEmail, ftxEmailMultiDeli, ftxMMNorthMgrEmail, ftxMMSouthMgrEmail)
  ON multi.fIdGlobal = src.fIdGlobal
;";
}

private static Dictionary<string, string[]> LoadReplacements()
{
    return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
      { "Jason Broderick", new[] { "JasonB@trailappliances.com", "JBroderick@trailappliances.com", "JasonB", "JBroderick" } },
      { "Brent Laturnus", new[] { "BrentL@trailappliances.com", "BLaturnus@trailappliances.com", "BrentL", "BLaturnus" } },
      { "Stephanie Krzyz", new[] { "StephanieK@trailappliances.com", "SKrzyz@trailappliances.com", "StephanieK", "SKrzyz" } },
      { "Richard Broderick", new[] { "RichardB@trailappliances.com", "RBroderick@trailappliances.com", "RichardB", "RBroderick" } },
      { "Trevor Love", new[] { "TrevorL@trailappliances.com", "TLove@trailappliances.com", "TrevorL", "TLove" } },
      { "Larry Law", new[] { "LARRYL@trailappliances.com", "LLAW@trailappliances.com", "LARRYL", "LLAW" } },
      { "purchasing@trailappliances.com", new[] { "purchasing@trailappliances.com", "purchasing" } },
      { "kelwarehouse@trailappliances.com", new[] { "kelwarehouse@trailappliances.com", "Kelowna Warehouse" } },
    };
}

/// <summary>
/// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another 
/// specified string according the type of search to use for the specified string.
/// </summary>
/// <param name="str">The string performing the replace method.</param>
/// <param name="oldValue">The string to be replaced.</param>
/// <param name="newValue">The string replace all occurrences of <paramref name="oldValue"/>. 
/// If value is equal to <c>null</c>, than all occurrences of <paramref name="oldValue"/> will be removed from the <paramref name="str"/>.</param>
/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search.</param>
/// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced with <paramref name="newValue"/>. 
/// If <paramref name="oldValue"/> is not found in the current instance, the method returns the current instance unchanged.</returns>
public static string Replace(string str,
    string oldValue, string @newValue,
    StringComparison comparisonType)
{

    // Check inputs.
    if (str == null)
    {
        // Same as original .NET C# string.Replace behavior.
        throw new ArgumentNullException(nameof(str));
    }
    if (str.Length == 0)
    {
        // Same as original .NET C# string.Replace behavior.
        return str;
    }
    if (oldValue == null)
    {
        // Same as original .NET C# string.Replace behavior.
        throw new ArgumentNullException(nameof(oldValue));
    }
    if (oldValue.Length == 0)
    {
        // Same as original .NET C# string.Replace behavior.
        throw new ArgumentException("String cannot be of zero length.");
    }


    //if (oldValue.Equals(newValue, comparisonType))
    //{
    //This condition has no sense
    //It will prevent method from replacesing: "Example", "ExAmPlE", "EXAMPLE" to "example"
    //return str;
    //}



    // Prepare string builder for storing the processed string.
    // Note: StringBuilder has a better performance than String by 30-40%.
    StringBuilder resultStringBuilder = new StringBuilder(str.Length);



    // Analyze the replacement: replace or remove.
    bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);



    // Replace all values.
    const int valueNotFound = -1;
    int foundAt;
    int startSearchFromIndex = 0;
    while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
    {

        // Append all characters until the found replacement.
        int @charsUntilReplacment = foundAt - startSearchFromIndex;
        bool isNothingToAppend = @charsUntilReplacment == 0;
        if (!isNothingToAppend)
        {
            resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilReplacment);
        }



        // Process the replacement.
        if (!isReplacementNullOrEmpty)
        {
            resultStringBuilder.Append(@newValue);
        }


        // Prepare start index for the next search.
        // This needed to prevent infinite loop, otherwise method always start search 
        // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
        // and comparisonType == "any ignore case" will conquer to replacing:
        // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
        startSearchFromIndex = foundAt + oldValue.Length;
        if (startSearchFromIndex == str.Length)
        {
            // It is end of the input string: no more space for the next search.
            // The input string ends with a value that has already been replaced. 
            // Therefore, the string builder with the result is complete and no further action is required.
            return resultStringBuilder.ToString();
        }
    }


    // Append the last part to the result.
    int @charsUntilStringEnd = str.Length - startSearchFromIndex;
    resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilStringEnd);


    return resultStringBuilder.ToString();

}