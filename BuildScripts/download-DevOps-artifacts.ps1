####
#
# This script is working. It is based on another from here: https://github.com/Azure/azure-devtestlab/blob/master/Artifacts/windows-vsts-download-and-run-script/DownloadVstsDropAndExecuteScript.ps1
# It has been altered to use C# style requests, rather than Invoke-RestMethod, couldnt get that working. (maybe the headers?)
#
# This is a test to:
#  - download the artifacts of a given Azure Devops build as a zip
#  - extract the zip 
#
####

[CmdletBinding()]
param(
    [string] $AccessToken = "asdfasdfasdfasdfasdfasdfasdfasdf",
    [string] $buildDefinitionName = "SomeBuild-CI",
    [string] $buildId = "1111",
    [string] $artifactName = "drop",
    [string] $vstsProjectUri = "https://dev.azure.com/OrgName/ProjName",
    [string] $pathToScript,
    [string] $scriptArguments,
    [string] $buildDefId
)

###################################################################################################
#
# PowerShell configurations
#

# NOTE: Because the $ErrorActionPreference is "Stop", this script will stop on first failure.
#       This is necessary to ensure we capture errors inside the try-catch-finally block.
$ErrorActionPreference = "Stop"

# Ensure we force use of TLS 1.2 for all downloads.
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Ensure we set the working directory to that of the script.
Push-Location $PSScriptRoot

# Configure strict debugging.
Set-PSDebug -Strict

###################################################################################################
#
# Handle all errors in this script.
#

trap
{
    # NOTE: This trap will handle all errors. There should be no need to use a catch below in this
    #       script, unless you want to ignore a specific error.
    $message = $error[0].Exception.Message
    if ($message)
    {
        Write-Host -Object "ERROR: $message" -ForegroundColor Red
    }
    
    # IMPORTANT NOTE: Throwing a terminating error (using $ErrorActionPreference = "Stop") still
    # returns exit code zero from the PowerShell script when using -File. The workaround is to
    # NOT use -File when calling this script and leverage the try-catch-finally block and return
    # a non-zero exit code from the catch block.
    exit -1
}

###################################################################################################
#
# Functions used in this script.
#

function Get-BuildArtifacts
{
    [CmdletBinding()]
    param (
        [string] $ArtifactsUri,
        [string] $Destination
    )

    # Clean up destination path first, if needed.
    if (Test-Path $Destination -PathType Container)
    {
        Write-Host "Cleaning up destination folder $Destination"
        Remove-Item -Path $Destination -Force -Recurse | Out-Null
    }

    Add-Type -AssemblyName System.Net.Http
    $basicAuth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "Powershell", $AccessToken)))
    $httpClientHandler = New-Object System.Net.Http.HttpClientHandler
    $httpClient = New-Object System.Net.Http.Httpclient $httpClientHandler
    $httpClient.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Basic", $basicAuth)
    $mediaTypeValue = New-Object System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
    $httpClient.DefaultRequestHeaders.Accept.Add($mediaTypeValue)

    $response = $httpClient.GetAsync($ArtifactsUri).Result
    $response.EnsureSuccessStatusCode()
    $body = $response.Content.ReadAsStringAsync().Result | ConvertFrom-Json

    $artifactName = $body.name
    $downloadLink = $body.resource.downloadUrl

    $mediaTypeValue = New-Object System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/zip")
    $httpClient.DefaultRequestHeaders.Accept.Clear()
    $httpClient.DefaultRequestHeaders.Accept.Add($mediaTypeValue)

    $response = $httpClient.GetAsync($downloadLink).Result
    $response.EnsureSuccessStatusCode()
    
    $artifactZip = "$artifactName.zip"
    New-Item -Path "$Destination" -Type Directory -Force | Out-Null
    $fileStream = New-Object System.IO.FileStream("$Destination\$artifactZip", [System.IO.FileMode]::CreateNew)
    $response.Content.CopyToAsync($fileStream).Result

    $fileStream.Dispose()    
    $httpClient.Dispose()
    
    Write-Host "Extracting artifact file $artifactZip to $Destination"
    [System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null 
    [System.IO.Compression.ZipFile]::ExtractToDirectory("$Destination\$artifactZip", $Destination) | Out-Null
}

function Get-BuildDefinitionId
{
    [CmdletBinding()]
    param (
        [string] $BuildDefinitionUri,
        [Hashtable] $Headers
    )

    Write-Host "Getting build definition ID from $BuildDefinitionUri"
    $buildDef = Invoke-RestMethod -Uri $BuildDefinitionUri -Headers $Headers -Method Get
    if (-not $buildDefId)
    {
     $buildDefinitionId = $buildDef.value.id[0]
     if (-not $buildDefinitionId)
      {
          throw "Unable to get the build definition ID from $buildDefinitionUri"
      }
    }
    else
    {
        $buildDefinitionId = $buildDefId
    }
    
   

    return $buildDefinitionId
}

function Get-LatestBuildId
{
    param (
        [string] $BuildUri,
        [Hashtable] $Headers
    )

    Write-Host "Getting latest build ID from $BuildUri"
    $builds = Invoke-RestMethod -Uri $BuildUri -Headers $Headers -Method Get | ConvertTo-Json | ConvertFrom-Json
    $buildId = $builds.value[0].id
    if (-not $buildId)
    {
        throw "Unable to get the latest build ID from $BuildUri"
    }

    return $buildId
}
 
function Invoke-Script
{
    [CmdletBinding()]
    param (
        [string] $Path,
        [string] $Script,
        [string] $Arguments
    )

    $scriptPath = Join-Path -Path $Path -ChildPath $Script

    Write-Host "Running $scriptPath"

    if (Test-Path $scriptPath -PathType Leaf)
    {
        Invoke-Expression "& `"$scriptPath`" $Arguments"
    }
    else
    {
        Write-Error "Unable to locate $scriptPath"
    }
}

function Set-AuthHeaders
{
    [CmdletBinding()]
    param (
        [string] $UserName = "",
        [string] $AccessToken
    )

    $basicAuth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $UserName,$AccessToken)))
    return @{ Authorization = "Basic $basicAuth" }
}

###################################################################################################
#
# Main execution block.
#

try
{
    # Prepare values used throughout.
    $vstsApiVersion = "4.1"
    $downloadRootFolder = "c:\temp\downloads"
    $destination = "$downloadRootFolder\$buildDefinitionName"
    $vstsProjectUri = $vstsProjectUri.TrimEnd("/")

    # Output provided parameters.
    Write-Host 'Provided parameters used in this script:'
    Write-Host "  `$accessToken = $('*' * $accessToken.Length)"
    Write-Host "  `$buildDefinitionName = $buildDefinitionName"
    Write-Host "  `$vstsProjectUri = $vstsProjectUri"
    Write-Host "  `$pathToScript = $pathToScript"
    Write-Host "  `$scriptArguments = $scriptArguments"

    # Output constructed variables.
    Write-Host 'Variables used in this script:'
    Write-Host "  `$vstsApiVersion = $vstsApiVersion"
    Write-Host "  `$outfile = $outfile"
    Write-Host "  `$destination = $destination"

    # Download the build artifact package.
    $artifactsUri = "$vstsProjectUri/_apis/build/builds/$buildId/artifacts?artifactName=$artifactName&api-version=$vstsApiVersion"
    Get-BuildArtifacts -ArtifactsUri $artifactsUri -Destination $destination
}
finally
{
    Pop-Location
}


