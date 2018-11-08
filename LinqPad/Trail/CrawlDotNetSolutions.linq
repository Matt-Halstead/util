<Query Kind="Program" />

// This app finds all solutions and cs/vb projects, all dependencies within them,
// and creates output to help visualise the dependencies.
//

private static readonly string[] SupportedProjectExtensions = new[] { ".csproj", ".vbproj", ".rptproj", ".dtproj", ".sqlproj" };

private const string RootSrcFolder = @"E:\dev\src\";

void Main()
{
	// Parse solutions and projects from files, resolve refs.
	var projectsByGuid = ParseProjectFiles(RootSrcFolder, AcceptPath);
	var solutionsByFile = ParseSolutionFiles(RootSrcFolder, AcceptPath);
	projectsByGuid.Values.ToList().ForEach(proj => proj.ResolveProjectRefs(projectsByGuid));
	solutionsByFile.Values.ToList().ForEach(sln => sln.ResolveProjectRefs(projectsByGuid));

	// Output
	ListProjectsBySolution(solutionsByFile.Values.OrderBy(s => s.Name).ToList());
}

private static string[] ExcludeFolders = new[]
{
	@"E:\dev\src\BINS_OLD\",
	@"E:\dev\src\SCAN_OLD\",
	@"E:\dev\src\SCAN-hack\",
	@"E:\dev\src\CONFQ\ConfQuot\ConfQuotBackup",
	@"E:\dev\src\TRFR\TRFR\TrfrBackup",
};

private static bool AcceptPath(string path)
{	
	return !ExcludeFolders.Any(folder => path.StartsWith(folder));
}


private static void ListProjectsBySolution(List<Solution> solutions)
{
	Console.WriteLine($"\nLISTING PROJECTS per SOLUTION:\n");

	foreach (var solution in solutions)
	{
		Console.WriteLine($"\n{solution.Filename}:");
		
		foreach (var project in solution.Projects)
		{
			var message = project.IsResolved ? "ok." : project.ResolveError;
			Console.WriteLine($"  - {project.Name} [{project.ProjectType}] {project.Guid}: {message}");
		}
	}
}

private static Dictionary<ProjectReference, Project> ParseProjectFiles(string rootFolder, Func<string, bool> acceptFilterFunc)
{
	Console.WriteLine($"PARSING PROJECTS under folder: {rootFolder}\n");

	// Find all supported projects.
	var projFiles = Directory.GetFiles(rootFolder, "*.*proj", SearchOption.AllDirectories);
	
	var unsupportedProjFiles = projFiles
		.Where(file => !SupportedProjectExtensions.Contains(Path.GetExtension(file ?? string.Empty)?.ToLower() ?? string.Empty))
		.ToArray();
	Console.WriteLine($"There were {unsupportedProjFiles.Length} projects with unsupported extensions.");
	foreach (var file in unsupportedProjFiles)
	{
		Console.WriteLine($"  - {file}");
	}
	Console.WriteLine();
	
	var excludedPaths = projFiles.Where(path => !acceptFilterFunc(path)).ToArray();
	Console.WriteLine($"EXCLUDED {excludedPaths.Length} projects.");
	foreach (var file in excludedPaths)
	{
		Console.WriteLine($"  - {file}");
	}
	Console.WriteLine();

	var projects = projFiles
		.Except(excludedPaths)
		.Except(unsupportedProjFiles)
		.Select(projFile => new Project(projFile))
		.ToArray();
		
	var maxNameLength = (projects
		.OrderBy(proj => proj.Name.Length)
		.Last()
		?.Name ?? string.Empty)
		.Length;

	var projectsLookup = new Dictionary<ProjectReference, Project>(new ProjectReferenceEqualityComparer());

	// Group by GUID first to detect collisions.
	var projectsByGuid = projects
		.GroupBy(p => p.Guid, StringComparer.OrdinalIgnoreCase)
		.OrderBy(group => group.Count())
		.ToArray();

	foreach (var projectGroup in projectsByGuid)
	{
		if (projectGroup.Count() == 1)
		{
			var proj = projectGroup.First();
			Console.WriteLine($"Project {projectGroup.Key} {proj.Name.PadRight(maxNameLength)}: {proj.Filename}");
		}
		else
		{
			maxNameLength = projectGroup.OrderBy(proj => proj.Name.Length).Last().Name.Length;
		
			Console.WriteLine($"WARNING: Project GUID {projectGroup.Key} used in multiple projects:");
			foreach (var project in projectGroup)
			{
				Console.WriteLine($"  - {project.Name.PadRight(maxNameLength)}: {project.Filename}");
			}
		}

		foreach (var project in projectGroup)
		{
			projectsLookup[project.GetReference()] = project;
		}
	}

	return projectsLookup;
}

private static Dictionary<string, Solution> ParseSolutionFiles(string rootFolder, Func<string, bool> acceptFilterFunc)
{
	Console.WriteLine($"\nPARSING SOLUTIONS under folder: {rootFolder}\n");

	var slnFiles = Directory.GetFiles(rootFolder, "*.sln", SearchOption.AllDirectories)
		.Where(path => acceptFilterFunc(path))
		.ToArray();

	var excluded = slnFiles.Where(path => !acceptFilterFunc(path)).ToArray();
	Console.WriteLine($"EXCLUDED {excluded.Length} solutions.");
	foreach (var file in excluded)
	{
		Console.WriteLine($"  - {file}");
	}
	Console.WriteLine();

	// todo, unsafe if dup keys
	var solutions = slnFiles
		.Except(excluded)
		.Select(file => new Solution(file))
		.ToArray();
	
	return solutions.ToDictionary(sln => sln.Filename, sln => sln, StringComparer.OrdinalIgnoreCase);
}





public enum ProjectType { Unknown, CSharp, VisualBasic, Report, IntegrationServices, Sql };

class ProjectReferenceEqualityComparer : IEqualityComparer<ProjectReference>
{
	public int GetHashCode(ProjectReference ref1) => ref1?.ToString()?.GetHashCode() ?? 0;

	public bool Equals(ProjectReference ref1, ProjectReference ref2)
	{
		var key1 = ref1?.ToString();
		var key2 = ref2?.ToString();
		return string.Equals(key1, key2);
	}
}

class ProjectReference
{
	public ProjectReference(string guid, string path, string name, ProjectType type)
	{
		Guid = guid;
		RelativePath = path;
		Name = name;
		ProjectType = type;
	}

	public static ProjectType GetProjectType(string filename)
	{
		var result = ProjectType.Unknown;
		var ext = Path.GetExtension(filename ?? string.Empty)?.ToLower().Trim() ?? string.Empty;

		switch (ext)
		{
			default:
				break;

			case ".csproj":
				result = ProjectType.CSharp;
				break;

			case ".vbproj":
				result = ProjectType.VisualBasic;
				break;

			case ".rptproj":
				result = ProjectType.Report;
				break;

			case ".dtproj":
				result = ProjectType.IntegrationServices;
				break;

			case ".sqlproj":
				result = ProjectType.Sql;
				break;
		}

		return result;
	}

	public override string ToString() => $"{Guid}-{Name}";

	public string Guid { get; private set; }
	public string RelativePath { get; private set; }
	public string Name { get; private set; }
	public ProjectType ProjectType { get; private set; }

	public string ResolveError { get; private set; } = string.Empty;
	
	public bool IsResolved => string.IsNullOrEmpty(ResolveError);

	// Try to resolve this reference to one of a known list of found projects.
	// Must be same GUID, name and rel path.
	public bool TryResolve(Dictionary<ProjectReference, Project> projectsLookup)
	{
		ResolveError = $"UNRESOLVED, unspecified error.";
		
		if (projectsLookup.TryGetValue(this, out Project project))
		{
			if (Name == project.Name)
			{
				ResolveError = string.Empty;
			}
			else
			{
				ResolveError = $"UNRESOLVED, name '{Name}' mismatch, expected '{project.Name}'.";
			}
		}
		else if (!SupportedProjectExtensions.Contains(Path.GetExtension(RelativePath)))
		{
			ResolveError = $"UNRESOLVED, '{Name}' {Guid} has unsupported type {ProjectType}.";
		}
		else
		{
			ResolveError = $"UNRESOLVED, '{Name}' {Guid} unknown.";
		}

		return IsResolved;
	}
}

class Project
{
	private readonly List<ProjectReference> _projRefs = new List<ProjectReference>();

	public Project(string filename)
	{
		Filename = filename;
		ParseIDFromXmlFile();
	}

	public string Guid { get; private set; }
	public string Filename { get; private set; }
	public string Name { get; private set; }

	public ProjectType ProjectType => ProjectReference.GetProjectType(Filename);
	
	public ProjectReference[] ProjectReferences => _projRefs.ToArray();

	// todo, Filename is not the path, fix this!
	public ProjectReference GetReference() => new ProjectReference(Guid, Filename, Name, ProjectType);

	// Resolves the ProjectReferences in the sln file with the actual projects found previously.
	public void ResolveProjectRefs(Dictionary<ProjectReference, Project> projectsLookup)
	{
		var doc = XDocument.Load(Filename);
		var ns = doc.Root.Name.Namespace;

		var projectRefNodes = doc.Root.Descendants(ns + "ProjectReference");
		foreach (var projNode in projectRefNodes)
		{
			var projectPath = projNode.Attribute("Include")?.Value ?? string.Empty;
			var projectGuid = projNode.Element(ns + "Project")?.Value ?? string.Empty;
			var projectName = projNode.Element(ns + "Name")?.Value ?? string.Empty;

			var projRef = new ProjectReference(projectGuid, projectPath, projectName, ProjectType);
			projRef.TryResolve(projectsLookup);
			_projRefs.Add(projRef);
		}
	}

	private void ParseIDFromXmlFile()
	{
		var doc = XDocument.Load(Filename);
		var ns = doc.Root.Name.Namespace;
		
		var firstGroup = doc.Root.Element(ns + "PropertyGroup");
		if (firstGroup != null)
		{
			Guid = firstGroup.Element(ns + "ProjectGuid")?.Value ?? string.Empty;
			Name = firstGroup.Element(ns + "AssemblyName")?.Value ?? string.Empty;
		}
		
		// HACK
		if (ProjectType == ProjectType.Report || ProjectType == ProjectType.IntegrationServices)
		{
			Guid = string.IsNullOrEmpty(Guid) ? "{00000000-0000-0000-0000-000000000000}" : Guid;
			Name = string.IsNullOrEmpty(Name) ? Path.GetFileNameWithoutExtension(Filename) : Name;
		}
	}
}





class Solution
{
	private readonly List<ProjectReference> _projRefs = new List<ProjectReference>();

	public Solution(string filename)
	{
		Filename = filename;
		Name = Path.GetFileNameWithoutExtension(Filename);
	}

	public string Name { get; private set; }
	public string Filename { get; private set; }

	public ProjectReference[] Projects => _projRefs.ToArray();

	public void ResolveProjectRefs(Dictionary<ProjectReference, Project> projectsLookup)
	{
		// find projects in sln file matching this form:
		// Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BinScanLib", "BinScanLib\BinScanLib.csproj", "{B7F354FC-5AFE-4CC6-80E9-D7574429FDEA}"
		var extensionsPattern = string.Join("|", SupportedProjectExtensions.Select(ext => $".+{ext}"));
		var regex = new Regex(@"Project\(""(\{.+\})""\) = ""(.+)"", ""(" + extensionsPattern + @")"", ""(\{.+\})""");
		
		foreach (var line in File.ReadAllLines(Filename))
		{
			var match = regex.Match(line);
			if (match.Captures.Count == 1 && match.Groups.Count == 5)
			{
				//var slnProjectGuid = match.Groups[1].Value;
				var projectName = match.Groups[2].Value;
				var projectPath = match.Groups[3].Value;
				var projectGuid = match.Groups[4].Value;
				
				var projRef = new ProjectReference(projectGuid, projectPath, projectName, ProjectReference.GetProjectType(projectPath));
				projRef.TryResolve(projectsLookup);
				_projRefs.Add(projRef);
			}
		}
	}
}