<Query Kind="Statements">
  <Connection>
    <ID>803b02c6-9d1e-42ce-ad5d-4ee099635351</ID>
    <Persist>true</Persist>
    <Server>trailsql</Server>
    <Database>TrailX</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

///
// The table [Various].[dbo].[appsList] is that accessed by TrailMix when allowing user to make a new announcement
// associates with a given app.
//

IEnumerable<dynamic> result = ExecuteQueryDynamic(@"
SELECT folder, location, name, appType, comments, eNet--, *
FROM [Various].[dbo].[appsList]
ORDER BY appsList.name
").ToArray();

result.Dump("[Various].[dbo].[appsList]");