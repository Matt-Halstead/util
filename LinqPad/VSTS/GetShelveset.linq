<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	Go();
}

private static async void Go()
{
	var projectUrl = @"https://mysite.visualstudio.com";
	var personalAccessToken = "xxxx";
	//var projects = Get(personalAccessToken, projectUrl, "/_apis/projects");
	var shelves = await Get(personalAccessToken, projectUrl, "/_apis/tfvc/shelvesets",
		Tuple.Create("requestData.owner", "Matt Halstead"),
		Tuple.Create("requestData.name", "myShelvesetsName"),
		Tuple.Create("api-version", "4.1"));

	//Console.WriteLine(shelves);

	var shelvesetContents = await Get(personalAccessToken, projectUrl, "/_apis/tfvc/shelvesets/changes",
		Tuple.Create("shelvesetId", "myShelvesetsName;fd09dec3-85da-44a5-a933-fd4fdde4bcd9"),
		Tuple.Create("api-version", "4.1"));
		
	DownloadShelvesetContent("myShelvesetsName", shelvesetContents, personalAccessToken);
}

private static Task<string> Get(string personalAccessToken, string projectUrl, string endPoint, params Tuple<string, string>[] queryArgs)
{
	return Task.Run(async () =>
	{
		string responseBody = string.Empty;
		
		try
		{
			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				string personalAccessTokenBase64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", personalAccessTokenBase64);

				string query = FormatQueryString(queryArgs);
				using (HttpResponseMessage response = await client.GetAsync($"{projectUrl}{endPoint}{query}"))
				{
					response.EnsureSuccessStatusCode();
					responseBody = await response.Content.ReadAsStringAsync();
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		
		return responseBody;
	});
}

private static async void DownloadShelvesetContent(string shelvesetId, string json, string personalAccessToken)
{
	//Console.WriteLine(json);

	var shelvesetFolder = $@"C:\Source\_shelvesets\{shelvesetId}";

	if (JsonConvert.DeserializeObject(json) is JObject jObject)
	{
		if (jObject.TryGetValue("value", out JToken jValue))
		{
			var sb = new StringBuilder();
			
			using (HttpClient client = new HttpClient())
			{
				string personalAccessTokenBase64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}"));
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", personalAccessTokenBase64);

				foreach (var jTokenValue in jValue.ToArray())
				{
					string changeType = jTokenValue["changeType"].ToString();

					var item = jTokenValue["item"];
					var version = item["version"];
					var path = item["path"].ToString();
					var url = item["url"].ToString();

					sb.AppendLine($"{changeType}: {path}");

					if (new[] { "add", "edit" }.Contains(changeType))
					{
						var destPath = path.Replace('/', '\\').Replace("$", shelvesetFolder);

						EnsureFolderExists(Path.GetDirectoryName(destPath));

						using (var request = new HttpRequestMessage(HttpMethod.Get, url))
						{
							using (
								Stream contentStream = await(await client.SendAsync(request)).Content.ReadAsStreamAsync(),
								stream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
							{
								await contentStream.CopyToAsync(stream);
							}
						}
					}
				}
				
				File.WriteAllText(Path.Combine(shelvesetFolder, "shelveset_info.txt"), sb.ToString());
			}
		}
	}
}

private static string FormatQueryString(params Tuple<string, string>[] queryArgs)
{
	var queryString = string.Join("&", queryArgs.Select(query => $"{query.Item1}={HttpUtility.UrlEncode(query.Item2)}"));
	queryString = string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
	return queryString;
}

private static void EnsureFolderExists(string path)
{
	var folders = path.Split('\\');
	var current = string.Empty;
	foreach (var folder in folders)
	{
		current = Path.Combine(current, folder);
		if (current.EndsWith(":"))
		{
			current += "\\";
		}

		if (!Directory.Exists(current))
		{
			Directory.CreateDirectory(current);
		}
	}
}
