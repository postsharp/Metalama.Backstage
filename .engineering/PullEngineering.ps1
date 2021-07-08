# Stop after first error.
$ErrorActionPreference = "Stop"

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root! Usage: Copy this file to the root of the repository and execute. The file deletes itself upon success."
}

# Update/initialize the engineering subtree.
$EngineeringDirectory = ".engineering"

& git subtree pull --prefix $EngineeringDirectory https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash
