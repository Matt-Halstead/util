<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
</Query>

void Main()
{
	// to define this text, put this url into a browser and save the JSON response to file:   http://trailjira.trailappliances.com:7990/rest/api/1.0/repos
	string json = File.ReadAllText(@"E:\work\Database Apps\BitBucket-repos.json");
	var repos = GetRepoInfo(json);

	foreach (var repo in repos)
	{
		Console.WriteLine($@"git clone {repo.HttpGitUrl} ""{repo.ProjectName}\{repo.RepoName}""");
	}
}

static IEnumerable<RepoInfo> GetRepoInfo(string json)
{
	var repoList = new List<RepoInfo>();
	
	if (JsonConvert.DeserializeObject(json) is JObject jsonObject)
	{
		foreach (var repo in jsonObject["values"])
		{
			var repoName = repo["name"].ToString();

			var project = repo["project"];
			var projectName = project["name"].ToString();

			var repoLinks = repo["links"];
			var httpCloneLink = repoLinks["clone"].FirstOrDefault(tok => tok.Value<string>("name") == "http");

			var cloneLink = httpCloneLink != null ? httpCloneLink["href"].ToString() : string.Empty;
			
			repoList.Add(new RepoInfo { RepoName = repoName, ProjectName = projectName, HttpGitUrl = new Uri(cloneLink) });
		}
	}
	
	return repoList;
}

struct RepoInfo
{
	public string RepoName;
	public string ProjectName;
	public Uri HttpGitUrl;
}