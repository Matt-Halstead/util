<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
</Query>

void Main()
{
    GetBuilds();
}

private static readonly string Organization = "trailtechworks";
private static readonly string Project = "DevOps";
private static readonly string PersonalAccessToken = "bmtdft3wjw64hqtjoupfyqcz4jlxxfxtffbklv5kyqkc6552dqvq";
private static readonly string UserName = "TrailBuild-Build-Agent";

public static async void GetBuilds()
{
    try
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var base64Pat = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{UserName}:{PersonalAccessToken}"));
                
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Pat);

            var url = $"https://dev.azure.com/{Organization}/{Project}/_apis/build/builds?api-version=5.0";

            var curl = $"curl -u {UserName}[:{PersonalAccessToken}] {url}";
            Console.WriteLine(curl);
            
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response follows:\n{responseBody}");
                Console.WriteLine($"<End of response>");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}