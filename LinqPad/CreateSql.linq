<Query Kind="Program" />

void Main()
{
	var tables = new[] {
"account",
"accounttype",
"anatomycheckevent",
"calibrationplate",
"casedata",
"casedatatype",
"caseschedule",
"casetreatment",
"casetreatmentautosaved",
"casetype",
"caseuser",
"drillburr",
"drilldiameterchangeevent",
"drilllengthmeasurementevent",
"drillpathdata",
"drilltype",
"handpiece",
"handpiecemodel",
"handpiecetype",
"jawtype",
"logevent",
"logeventdetail",
"logeventlevel",
"manufacturer",
"patientcase",
"patientcaseautosaved",
"patientcaseoption",
"protocol",
"protocoldetail",
"referenceconfiguration",
"registrationconfidence",
"registrationstate",
"screencontent",
"screentype",
"session",
"sessionscreenshot",
"sessiontype",
"sharedcasestatus",
"sharedcasestatustype",
"singlecameracalibration",
"stereocalibration",
"stereocalibrationparameter",
"tooth",
"transferfunction",
"treatment",
"treatmentbrand",
"treatmentfamily",
"treatmenttype",
"userpreference",
"usersetting",
"viewstate"
	};

	var userTables = new[] {
"account",
"treatment",
"casedata",
"usersetting",
"patientcase",
"patientcaseoption",
"viewstate",
"casetreatment",
"patientcaseautosaved",
"casetreatmentautosaved",
"caseuser",
"session",
"anatomycheckevent",
"drilllengthmeasurementevent",
"drillpathdata",
"drilldiameterchangeevent",
"sharedcasestatus",
"logevent",
"logeventdetail"
	};
	
	var sb = new StringBuilder();
	
	var domainTables = tables.Except(userTables).ToList();
	
	foreach (var table in domainTables)
	{
		sb.AppendLine(TableToString(table));
	}
	
	sb.ToString().Dump();
}

private static string TableToString(string tableName)
{
	return $@"------------------------------------------------
-- {tableName}
--
INSERT INTO `seymour`.{tableName} SELECT * FROM `seymour_2.4`.{tableName};
";
}
