<Query Kind="Program" />

void Main()
{
	string topFolder = @"\\build-fs\Build\MineSched\Main_Gated";
	
	var buildFolders = Directory.GetDirectories(topFolder, "*")
		.Where(path => new DirectoryInfo(path).CreationTime < new DateTime(2014, 07, 28))
		.Where(path => Path.GetFileName(path).StartsWith("9"));
	
	var installerFiles = new List<string>();
	
	foreach (var buildFolder in buildFolders)
	{
		var files = Directory.GetFiles(buildFolder, "*.zip", SearchOption.AllDirectories)
			.Where(path => !path.EndsWith("MineSchedHelp.zip"))
			.Where(path => new FileInfo(path).Length > 1000000)
			.ToArray();
			
		if (files.Any())
		{
			var version = ParseFilenameForVersion(buildFolder);
			
			files.Dump(string.Format("Installers in {0}, version {1}", buildFolder, version));
			
			foreach (var file in files)
			{
				string processorArchitecture = file.Contains("x64") ? "_x64" : "_x86";
				var line = string.Format("copy {0} %LOCAL_MSI_FOLDER%\\MineSched_{1}{2}.zip", file.Replace(topFolder, "%BUILD_DROP_FOLDER%"), version.Replace(".", "-"), processorArchitecture);
				installerFiles.Add(line);
			}
		}
	}
	
	installerFiles.Dump(string.Format("ALL Installers in {0}", topFolder));
}


/// Below is from C:\work\source\MSched.Development\Development\tools-dev\Tools\AutomatedTesting\AutomatedTesting.Core\TestDataController.cs ...

/// <summary>
/// Parse the file path for a MineSched installer for version 8.0.2 onwards and extract version text.
/// Here is what installer filename look like: Release build: MineSched_8-0-2_install.exe. QC build:  MineSched_8-0-399-2-QC_install.exe
/// If no valid version can be found, will attempt to parse a version in the legacy format.
/// </summary>
/// <param name="filename">File path for the MineSched installer for version 8.0.2 onwards.</param>
/// <returns>The version text embedded as part of the file name.</returns>
internal static string ParseFilenameForVersion(string filename)
{
 //var input = Path.GetFileNameWithoutExtension(filename ?? string.Empty) ?? string.Empty;

 var input = filename.Replace("minesched_", string.Empty);
 input = input.Replace("_nsis", string.Empty);
 input = input.Replace("_install", string.Empty);

 if (!string.IsNullOrWhiteSpace(input))
 {
   // form: major.minor.build.revision.type
   var match = Regex.Match(input, @"(\d+)[\.\-_](\d+)[\.\-_](\d+)[\.\-_](\d+)[\.\-_]([^\.\-_]+)", RegexOptions.IgnoreCase);
   if (match.Success)
   {
     // if type is non-number value then this is an old build and should be interpretted with legacy method.
     int typeAsInt;
     if (int.TryParse(match.Groups[5].Value, out typeAsInt))
     {
       return ParseLegacyFilenameForVersion(filename);
     }

     return string.Join(".", new[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value });
   }

   // form: major.minor.build.revision
   match = Regex.Match(input, @"(\d+)[\.\-_](\d+)[\.\-_](\d+)[\.\-_](\d+)", RegexOptions.IgnoreCase);
   if (match.Success)
   {
     return string.Join(".", new[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value });
   }

   // form: major.minor.revision
   match = Regex.Match(input, @"(\d+)[\.\-_](\d+)[\.\-_](\d+)", RegexOptions.IgnoreCase);
   if (match.Success)
   {
     return string.Join(".", new[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value });
   }

   // defer to the old method if still no match
   return ParseLegacyFilenameForVersion(filename);
 }

 return string.Empty;
}

internal static string ParseLegacyFilenameForVersion(string filename)
{
 string version = string.Empty;
 var p = filename ?? string.Empty;

 p = Path.GetFileNameWithoutExtension(p);
 p = p.Replace("_alpha_", "-");
 p = p.Replace("_beta_", "-");

 p = p.Replace("_interim_", "_");
 p = p.Replace("_light_", "_");
 p = p.Replace("_full_", "_");
 p = p.Replace("_install", "_");

 var parts = p.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

 // the first part should be minesched.
 if (parts[0] == "minesched" || parts[0] == "autominesched")
 {
   // check for an old style version string.

   // this first part checks for an old style filename, pre 7.1.0.0
   if (parts.Length >= 5)
   {
     // if the second last part is "build", then the third last part is the code change, and the last part is the revision number.
     // the next part is a 3 digit version string.
     version = parts[1];

     var cc = parts[parts.Length - 3];
     var build = parts[parts.Length - 2];
     var number = parts[parts.Length - 1];

     if (build == "build")
     {
       version = version + "-" + cc + "-" + number;
     }
   }
   else if (parts.Length > 1)
   {
     // we have changed our version numbering for version 7.1.0.0 onwards.
     // all builds provided here should have 5 or size numerical version parts.
     // It will either be:
     //     {major}.{minor}.{maintainence}.{revision}.{ccnet_build_number}
     // OR  {major}.{minor}.{maintainence}.{revision}.{codechange}.{ccnet_build_number}

     // because we're splitting on underscores (_), there are actually only three elements in parts:
     //  "minesched" : {version string} : "install"
     // so we just need the middle one.
     version = parts[1];
   }
 }

 version = version ?? Path.GetFileNameWithoutExtension(filename) ?? "unknown";
 version = version.Replace('-', '.');

 return version;
}