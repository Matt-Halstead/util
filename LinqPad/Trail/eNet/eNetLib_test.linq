<Query Kind="Program">
  <Reference Relative="..\..\..\..\..\Syspro\eNetClients-dev-enet\source\eNetLib\bin\Debug\eNetLib.dll">E:\dev\Syspro\eNetClients-dev-enet\source\eNetLib\bin\Debug\eNetLib.dll</Reference>
  <Namespace>eNetLib</Namespace>
</Query>

void Main()
{
    var db = new SysproDatabase("Data Source=LabSyspro7-TRAILSQL;Initial Catalog=SysproCompanyA;Integrated Security=True;Application Name=LinqPad-Test");
    db.FindSysproBranches().Dump();
}