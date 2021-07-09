
Param ( [Parameter(Mandatory=$True)] [string] $ProjectName )

# Stop after first error.
$ErrorActionPreference = "Stop"

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root!"
}

& SignClient Sign --baseDirectory .\publish\ --input *.nupkg --config .\.engineering\deploy\signclient-appsettings.json --name $ProjectName --user sign-caravela@postsharp.net --secret $Env:SIGNSERVER_SECRET

if (!$?) {
	throw "Signing failed."
}

& postsharp-eng nuget verify -d publish

if (!$?) {
	throw "Verification failed."
}