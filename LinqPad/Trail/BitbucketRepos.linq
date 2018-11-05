<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

/**
 * This script will get the list of projects in BitBucket, and then for each, get the list of repos, and optionally clone them all.
 *
 * To get projects: http://trailjira.trailappliances.com:7990/rest/api/1.0/projects
 * To get the repos within a project 'MyProject': http://trailjira.trailappliances.com:7990/rest/api/1.0/projects/MyProject/repos
 */

void Main()
{
	// to define this text, put this url into a browser and save the JSON response to file:   http://trailjira.trailappliances.com:7990/rest/api/1.0/repos
	//string json = File.ReadAllText(@"E:\work\Database Apps\BitBucket-repos.json");

	string bitBucketUrl = @"http://trailjira.trailappliances.com:7990";
	var repos = new List<RepoInfo>();

	// ensure we get all the projects, default results page size is only 25
	var args = new[] { Tuple.Create("limit", "500") };
	string json = GetBasicJsonResponse(bitBucketUrl, $@"/rest/api/1.0/projects", args).Result;
	
	foreach (var project in ParseProjectInfo(json))
	{
		string repoJson = GetBasicJsonResponse(bitBucketUrl, $@"/rest/api/1.0/projects/{project.Key}/repos", args).Result;
		repos.AddRange(ParseRepoInfo(project, repoJson));
	}

	Console.WriteLine("\n\nBATCH FILE CONTENT FOLLOWS...\n\n");

	Console.WriteLine($@":: Ensure all the destination project folders exist
set PROJECT_LIST={string.Join(" ", repos.Select(repo => repo.Project.Key).Distinct())}
for %%x in (%PROJECT_LIST%) do mkdir %%x

:: Clone everything!");

	foreach (var repo in repos)
	{
		Console.WriteLine($@"git clone {repo.HttpGitUrl} ""{repo.Project.Key}\{repo.Name}""");
	}
	Console.WriteLine("\n\npause");
}

static Task<string> GetBasicJsonResponse(string baseAddress, string endPoint, params Tuple<string, string>[] queryArgs)
{
	return Task.Run(async () =>
	{
		string responseBody = string.Empty;

		var cookieContainer = new CookieContainer();
		using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
		using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) })
		{
			//			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
			//			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			//			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
			//			client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
			//
			//			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			//			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
			//			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("REST-API", "1.0"));
			
			client.DefaultRequestHeaders.ConnectionClose = false;
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

			// These need to be copied from those recently used by the browser
			cookieContainer.Add(client.BaseAddress, new Cookie("_atl_bitbucket_remember_me", "YjczZWU5OWJkMjVkNzNjYTI3MWI1MjFlZDI5Mjk4MWVjOWE4YmY1Yjo5YTkzMjg5YWFmYmFjZTY2MTgyNDU4ZGZjYmZmNDQyNTA5OThlNWI4"));
			cookieContainer.Add(client.BaseAddress, new Cookie("JSESSIONID", "E0B1361319F1F91E54607EE5C9F2131A"));

			string query = FormatQueryString(queryArgs);
			using (HttpResponseMessage response = await client.GetAsync($"{baseAddress}{endPoint}{query}"))
			{
				response.EnsureSuccessStatusCode();
				responseBody = await response.Content.ReadAsStringAsync();
			}
		}
		
		return responseBody;
	});
}

private static string FormatQueryString(params Tuple<string, string>[] queryArgs)
{
	var queryString = string.Join("&", queryArgs.Select(query => $"{query.Item1}={HttpUtility.UrlEncode(query.Item2)}"));
	queryString = string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
	return queryString;
}

static IEnumerable<ProjectInfo> ParseProjectInfo(string json)
{
	var projects = new List<ProjectInfo>();

	if (JsonConvert.DeserializeObject(json) is JObject jsonObject)
	{
		foreach (var project in jsonObject["values"])
		{
			var projectKey = project["key"].ToString();
			var projectName = project["name"].ToString();

			projects.Add(new ProjectInfo { Key = projectKey, Name = projectName });
			Console.WriteLine($"Added project: {projectKey}");
		}
	}

	return projects;
}

static IEnumerable<RepoInfo> ParseRepoInfo(ProjectInfo projectInfo, string json)
{
	var repoList = new List<RepoInfo>();
	
	if (JsonConvert.DeserializeObject(json) is JObject jsonObject)
	{
		foreach (var repo in jsonObject["values"])
		{
			var repoName = repo["name"].ToString();
			var repoLinks = repo["links"];
			var httpCloneLink = repoLinks["clone"].FirstOrDefault(token => token.Value<string>("name") == "http");

			var cloneLink = httpCloneLink["href"]?.ToString() ?? string.Empty;
			if (!string.IsNullOrEmpty(cloneLink))
			{
				repoList.Add(new RepoInfo { Name = repoName, Project = projectInfo, HttpGitUrl = new Uri(cloneLink) });
			}
		}
	}
	
	return repoList;
}

struct ProjectInfo
{
	public string Key;
	public string Name;
}

struct RepoInfo
{
	public string Name;
	public Uri HttpGitUrl;
	public ProjectInfo Project;
}