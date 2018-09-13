<Query Kind="Program">
  <Reference>C:\Windows\System32\wuapi.dll</Reference>
  <Namespace>WUApiLib</Namespace>
</Query>

void Main()
{
	Type t = Type.GetTypeFromProgID("Microsoft.Update.Session", "locahost");
	UpdateSession uSession = (UpdateSession)Activator.CreateInstance(t);

	IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
	uSearcher.ServerSelection = ServerSelection.ssWindowsUpdate;
	uSearcher.IncludePotentiallySupersededUpdates = false;
	uSearcher.Online = false;

	ISearchResult sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0 And Type='Software'");
}