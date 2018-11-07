<Query Kind="Program" />

// This app finds all solutions and cs/vb projects, all dependencies within them,
// and creates output to help visualise the dependencies.

void Main()
{
	const string RootSrcFolder = @"E:\dev\src\";
	
	// Parse solutions and projects from files, resolve refs.
	var projectsByGuid = ParseProjectFiles(RootSrcFolder);
	var solutionsByName = ParseSolutionFiles(RootSrcFolder);	
	projectsByGuid.Values.ToList().ForEach(proj => proj.ResolveProjectRefs(projectsByGuid));
	solutionsByName.Values.ToList().ForEach(sln => sln.ResolveProjectRefs(projectsByGuid));

	// Build dependency graph
	

	// Output

}

private static Dictionary<string, Project> ParseProjectFiles(string rootFolder)
{
	var projectsByGuid = new Dictionary<string, Project>();

	var projFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories)
		.Concat(Directory.GetFiles(rootFolder, "*.vbproj", SearchOption.AllDirectories))
		.ToArray();

	Console.WriteLine($"PARSING PROJECTS under folder: {rootFolder}\n");

	foreach (var projFile in projFiles)
	{
		try
		{
			var project = new Project(projFile);
			if (!projectsByGuid.ContainsKey(project.Guid))
			{
				projectsByGuid[project.Guid] = project;
				Console.WriteLine($"Project: {projFile} --> {project.Guid}");
			}
			else
			{
				Console.WriteLine($"ERROR, project {project.Name} {project.Guid} already defined.  Skipped: {projFile}");
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"ERROR, skipped: {projFile}");
		}
	}

	return projectsByGuid;
}

private static Dictionary<string, Solution> ParseSolutionFiles(string rootFolder)
{
	var slnFiles = Directory.GetFiles(rootFolder, "*.sln", SearchOption.AllDirectories);
	var solutions = new List<Solution>();

	Console.WriteLine($"\nPARSING SOLUTIONS under folder: {rootFolder}\n");

	foreach (var sln in slnFiles)
	{
		try
		{
			solutions.Add(new Solution(sln));
			Console.WriteLine($"Solution: {sln}");
		}
		catch (Exception e)
		{
			Console.WriteLine($"ERROR, skipped: {sln}");
		}
	}
	
	return solutions.ToDictionary(sln => sln.Name, sln => sln);
}





public enum ProjectType { CSharp, VisualBasic };




class Project
{
	private readonly List<Project> _projRefs = new List<Project>();

	public Project(string filename)
	{
		Filename = filename;
		ParseIDFromXmlFile();
	}
	
	public string Filename { get; private set; }
	public string Guid { get; private set; }
	public string Name { get; private set; }
	public ProjectType ProjectType { get; private set; }

	public Project[] ProjectReferences => _projRefs.ToArray();

	public void ResolveProjectRefs(Dictionary<string, Project> projectsByGuid)
	{
		var doc = XDocument.Load(Filename);
		var ns = doc.Root.Name.Namespace;

		var projectRefNodes = doc.Root.Descendants(ns + "ProjectReference");
		foreach (var projNode in projectRefNodes)
		{
			var projectGuid = projNode.Element(ns + "Project")?.Value ?? string.Empty;
			var projectName = projNode.Element(ns + "Name")?.Value ?? string.Empty;

			if (projectsByGuid.TryGetValue(projectGuid, out Project project) && project.Name == projectName)
			{
				_projRefs.Add(project);
				Console.WriteLine($"Proj {Name}: ref added to {projectName}");
			}
			else
			{
				Console.WriteLine($"ERROR, skipped: '{projectName}'.  No project known with this name and guid {projectGuid}.");
			}
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
	}
}





class Solution
{
	private readonly List<Project> _projects = new List<Project>();

	public Solution(string filename)
	{
		Filename = filename;
		Name = Filename; // todo, is a better name ever needed?
	}

	public string Name { get; private set; }
	public string Filename { get; private set; }

	public Project[] Projects => _projects.ToArray();

	public void ResolveProjectRefs(Dictionary<string, Project> projectsByGuid)
	{
		// find projects in sln file matching this form:
		// Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BinScanLib", "BinScanLib\BinScanLib.csproj", "{B7F354FC-5AFE-4CC6-80E9-D7574429FDEA}"
		var regex = new Regex(@"Project\(""(\{.+\})""\) = ""(.+)"", ""(.+.csproj|.+.vbproj)"", ""(\{.+\})""");
		
		foreach (var line in File.ReadAllLines(Filename))
		{
			var match = regex.Match(line);
			if (match.Captures.Count == 1 && match.Groups.Count == 5)
			{
				//var slnProjectGuid = match.Groups[1].Value;
				var projectName = match.Groups[2].Value;
				//var projectPath = match.Groups[3].Value;
				var projectGuid = match.Groups[4].Value;
				
				if (projectsByGuid.TryGetValue(projectGuid, out Project project) && project.Name == projectName)
				{
					_projects.Add(project);
					Console.WriteLine($"Solution {Name}: ref added to {projectName}");
				}
				else
				{
					Console.WriteLine($"ERROR, skipped: '{projectName}'.  No project known with this name and guid {projectGuid}.");
				}
			}
		}
	}

	public void Add(IEnumerable<Project> projects)
	{
		_projects.AddRange(projects);
	}
}