<Query Kind="Program" />

void Main()
{
    var all = Raw.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        .ToArray();;
        
    var unique = all.Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
        
    unique.Dump();
}

// Define other methods and classes here
private static string Raw = @"creditdept@trailappliances.com
buildersales@trailappliances.com; sfbuilderadmin@trailappliances.com; creditdept@trailappliances.com; mfprojectcoordinators@trailappliances.com; cststaff@trailappliances.com
sfbuilderadmin@trailappliances.com; creditdept@trailappliances.com; mfprojectcoordinators@trailappliances.com
Each Builder Salesperson
creditdept@trailappliances.com; wendyu@trailappliances.com
jmaslove@trailappliances.com
mfprojectcoordinators@trailappliances.com; sfbuilderadmin@trailappliances.com
richardb,kvanvlack,brentl
Retired
richardb,kvanvlack
richard broderick; allen cheng; brentl; katherine van vlack
richard broderick; katherine van vlack; parisa zaini
Purchasing; katherine van vlack
Purchasing; terry volden; michael burchett; mike speckman
gsharpe@trailappliances.com; aptreplacement@trailappliances.com
stephaniek; katherine van vlack
Purchasing; mark pickering
richard broderick; james reynolds;trevor love;katherine van vlack
richard broderick;katherine van vlack
richard broderick; katherine van vlack
james reynolds; richard broderick; katherine van vlack
richard broderick; katherine van vlack
richard broderick; katherine van vlack
james reynolds; richard broderick; katherine van vlack
richard broderick; katherine van vlack; stephanie krzyz; kevin miao;kevyn peterson; parisa zaini
richard broderick
reports@trailspapp.trailappliances.com
Purchasing
richard broderick; katherine van vlack
richard broderick; katherine van vlack
Purchasing; brentl
Each showroom's manager
ema@trailappliances.com
jbroderick@trailappliances.com; dtruesdell@trailappliances.com; jgrant@trailappliances.com; Builders@TrailAppliances.com; purchasing; James Reynolds; Richard Broderick; SF Builder Administration; mfprojectcoordinators@trailappliances.com; CSTSFBuilder@trailappliances.com;CSTRichmond@trailappliances.com;CSTCoquitlam@trailappliances.com;CSTVancouver@trailappliances.com;CSTSurrey@trailappliances.com;CSTLangley@trailappliances.com;CSTClearance@trailappliances.com;CSTOPE@trailappliances.com
dispatcher@trailappliances.com; sfraser@trailappliances.com;receiving@trailappliances.com; warehousemanagers@trailappliances.com
purchasing; Kelowna Sales <kelsales@trailappliances.com>; Stephanie McAteer; Adam Kossack; Kelowna Managers <kelmanager@trailappliances.com>; CSTOPE@trailappliances.com; CSTKelowna@trailappliances.com
kelwarehouse@trailappliances.com; slatter@trailappliances.com;kelmanager@trailappliances.com;
purchasing; Stephanie McAteer; Adam Kossack;CSTOPE@trailappliances.com;CSTVictoria@trailappliances.com;rbest@trailappliances.com
vicwarehouse@trailappliances.com; Justin Gauthier <jgauthier@trailappliances.com>
Dispatcher; Stephanie Fraser; MF Builder Division; Richard Broderick; Vicki Yarwood; AW Supervisors; Receiving; Aileen Wong;BW Warehouse; MFPurchasing@trailappliances.com
terry volden; amanda burge; katherine van vlack
Dispatcher@trailappliances.com; SFraser@trailappliances.com; MfBuilderDiv; Richard Broderick;  Vicki Yarwood; Receiving; MFPurchasing@trailappliances.com; Kelowna Warehouse, Mike Speckman
Melissa Hofer
MF Builder Division; vicwarehouse@trailappliances.com; mflynn@trailappliances.com; Glenn Westwood; Dispatcher; Receiving; MFPurchasing@trailappliances.com
DELIVERY\DeliRep
CSTstaff@trailappliances.com; SF Builder Administration; reports@trailspapp.trailappliances.com
mfprojectcoordinators@trailappliances.com; reports@trailspapp.trailappliances.com
mfprojectcoordinators@trailappliances.com;  retailadmin@trailappliances.com; vicmanagers@trailappliances.com; kelmanager@trailappliances.com; 
angelah@trailappliances.com; reports@trailspapp.trailappliances.com
milanb
rbroderick@trailappliances.com
ema@trailappliances.com
mfirth@trailappliances.com
tlove@trailappliances.com
jerikson@trailappliances.com
retailmanagers@trailappliances.com
buildermanagement@trailappliances.com
jasonb@trailappliances.com; jamesr@trailappliances.com; dougt@trailappliances.com; trevorl@trailappliances.com;
buildermanagement@trailappliances.com
jasonb@trailappliances.com; jamesr@trailappliances.com; trevorl@trailappliances.com; reports@trailspapp.trailappliances.com; rbroderick@trailappliances.com
rmdmanagers@trailappliances.com; tlove@trailappliances.com; blaturnus@trailappliances.com; tlove@trailappliances.com; coqmanager@trailappliances.com; tlove@trailappliances.com; srymanagers@trailappliances.com; tlove@trailappliances.com; vanmanagers@trailappliances.com; tlove@trailappliances.com; lanmanagers@trailappliances.com; tlove@trailappliances.com; kelmanager@trailappliances.com; tlove@trailappliances.com; vicmanagers@trailappliances.com; tlove@trailappliances.com; clrmanager@trailappliances.com; tlove@trailappliances.com; abbotsfordmanagers@trailappliances.com; tlove@trailappliances.com; larryl@trailappliances.com; jerikson@trailappliances.com; dstefanucci@trailappliances.com
JasonB; RichardB; KVanVlack; TrevorL; BrentL; LarryL; GloriaC; MMcmanus;  reports@trailspapp.trailappliances.com
Jason Broderick; Richard Broderick; Trevor Love; Justin Erikson; Brent Laturnus; reports@trailspapp.trailappliances.com; Amy Shibasaki; Dino Stefanucci
carolw@trailappliances.com; janicem@trailappliances.com; reports@trailspapp.trailappliances.com
retailmanagers@trailappliances.com; buildermanagement@trailappliances.com
vharder@trailappliances.com; mmcmanus@trailappliances.com";