<Query Kind="Program" />

// This LINQPad program lets you define ranges of images within a large set of image pairs, and copy/import subsets of these
// into destination folders for use with ASTM testing, based on the index.
//
// It will also output text to assist with defining the imported ASTM tests in an xml document, as expected by the ASTM test app.
//
// Input: any number N of (source folder, dest folder, range)
// Output: N test folders containing images, xml defining those tests
//
// ASSUMPTIONS:
//  - Images are named with convention: <camera serial number>-<integer frame index>
//  - Images always occur in pairs, each with the same frame number and one or other of two distinct camera serial numbers.
//  - Image pairs are stored in subfolders below a root source folder, numbered by test point.
//
// TODO: This could easily be merged into the ASTM test app itself, would just need a few command line args.


// Root-level folder containing all the source images. 
const string RootSourceFolder = @"C:\Test\tracker_capture";

// Root-level folder to containing all the copied images in ASTM test folder structure.
const string RootDestFolder = @"C:\NavigateSurgicalPost\Tracker";
const int MaxRangeSize = 100;
const int Increment = 1;

void Main()
{
	// range tuple content: source hold id, dest test name, start frame
	// TODO: might need to add a source folder path to make this more flexible, and maybe a phantom angle
	var ranges = new[]
	{
		Tuple.Create(1, "Test1", 40),
	};

	CopyImagesTo(@"C:\NavigateSurgicalPost\Tracker\Temp5", @"C:\NavigateSurgicalPost\Tracker\Temp32");
	//CopyImages(ranges);
	//WriteXmlTestDefinitionFile(ranges);
}

static void CopyImagesTo(string fromPath, string dest)
{
	int[] omittedIndices = new[] { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45 };//, 43, 44, 45 };

	int i = 0;
	
	var dataFolder = new DataFolder(fromPath, ".png");
	
	foreach ((string left, string right, string leftId, string rightId, int index) in dataFolder.ImagePairs)
	{
		if (!omittedIndices.Contains(i))
		{
			if (!Directory.Exists(dest))
			{
				Directory.CreateDirectory(dest);
			}

			Copy(left, Path.Combine(dest, $"{leftId}-{index}.png"));
			Copy(right, Path.Combine(dest, $"{rightId}-{index}.png"));

			Console.WriteLine($"  index {index} --> image pair {index}.");
		}
		else
		{
			Console.WriteLine($"  skipped index {index} (i = {i})");
		}

		i++;
	}
}

static void CopyImages(IEnumerable<Tuple<int, string, int>> ranges)
{
	foreach (var range in ranges)
	{
		int[] omittedIndices = new[] { 5, 33, 43, 44, 45 };

		var source = Path.Combine(RootSourceFolder, range.Item1.ToString());
		var dest = Path.Combine(RootDestFolder, range.Item2);
		var start = range.Item3;

		Console.WriteLine($"Copying images starting at index {start} from {source}  ==>  {dest} ...");
		
		int i = 0;
		int n = 0;
		
		var dataFolder = new DataFolder(source, ".png");

		foreach ((string left, string right, string leftId, string rightId, int index) in dataFolder.ImagePairs)
		{
			if (index >= start || n >= MaxRangeSize)
			{
				break;
			}

			if (!omittedIndices.Contains(i))
			{
				if (!Directory.Exists(dest))
				{
					Directory.CreateDirectory(dest);
				}

				Copy(left, Path.Combine(dest, $"{leftId}-{n}.png"), true);
				Copy(right, Path.Combine(dest, $"{rightId}-{n}.png"), true);

				Console.WriteLine($"  index {index} --> image pair {n}.");

				i++;
				n++;
			}
			else
			{
				Console.WriteLine($"  skipped index {index}");
			}
		}
		Console.WriteLine($"Done, copied {n} images.");
	}
}

static void Copy(string source, string dest, bool dryRun = false)
{
	if (dryRun)
	{
		Console.WriteLine($"Copied {source} to: {dest}");
	}
	else
	{
		File.Copy(source, dest, true);
	}
}

// Copied from the TrackerASTMTests project.
internal class DataFolder
{
	private static readonly Regex _filenameRegex = new Regex(@"([0-9]+)-([0-9]+)", RegexOptions.Compiled);

	/// <summary>
	/// List of image pairs. Tuple content: (left file, right file, leftId, rightId, index)
	/// </summary>
	public List<Tuple<string, string, string, string, int>> ImagePairs { get; private set; }

	/// <summary>
	/// Get the image pairs list.
	/// </summary>
	/// <param name="path">Folder containing the image data.</param>
	public DataFolder(string path, string fileExtension)
	{
		var filenames = Directory.GetFiles(path, "*").Where(file => file.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase)).ToList();
		if (!filenames.Any())
		{
			throw new ApplicationException($"There are no images matching extension {fileExtension} in source folder {path}.");
		}

		ImagePairs = GetImagePairs(filenames);
	}

	/// <summary>
	/// Get the image pairs list.
	/// </summary>
	/// <param name="files">List of all the files in the folder.</param>
	/// <returns>Tuples containing paired image tuples (left file, right file, leftId, rightId, index), in ascending order of index.</returns>
	private List<Tuple<string, string, string, string, int>> GetImagePairs(IEnumerable<string> files)
	{
		// Original filename with the camera id and index extracted from filename.
		List<Tuple<string, int, int>> _splitFilenames = files.Select(SplitFilename).OrderBy(t => t.Item2).ThenBy(t => t.Item3).ToList();

		// group files by prefix
		var groups = _splitFilenames.GroupBy(t => t.Item2).ToList();

		if (groups.Count() != 2)
		{
			throw new ApplicationException($"There must be exactly two sets of images in the source folder.  Found: {string.Join(", ", _splitFilenames.Select(t => t.Item2).Distinct())}");
		}

		// Match pairs of images in the two groups by index, project to list of tuples of (left file, right file, leftId, rightId, index)
		return groups[0]
			.Join(groups[1], t => t.Item3, t => t.Item3, (l, r) => Tuple.Create(l.Item1, r.Item1, l.Item2.ToString(), r.Item2.ToString(), l.Item3))
			.ToList();
	}

	private Tuple<string, int, int> SplitFilename(string filename)
	{
		var success = TryParseImageFilenameElements(filename, out int prefix, out int index);
		return Tuple.Create(filename, success ? prefix : -1, success ? index : -1);
	}

	private bool TryParseImageFilenameElements(string filename, out int prefix, out int index)
	{
		prefix = -1;
		index = -1;
		var regex = _filenameRegex.Match(Path.GetFileNameWithoutExtension(filename));
		if (regex.Groups.Count >= 3)
		{
			string prefixStr = regex.Groups[1]?.Captures[0]?.Value ?? string.Empty;
			string indexStr = regex.Groups[2]?.Captures[0]?.Value ?? string.Empty;

			return int.TryParse(prefixStr, out prefix) && int.TryParse(indexStr, out index);
		}

		return false;
	}
}