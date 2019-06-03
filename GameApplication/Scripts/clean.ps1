Param
(
    [String]
    $ApplicationPackagePath,

    [String]
    $Configuration
)

. $PSScriptRoot\constants.ps1

if (!$Configuration)
{
    $Configuration = "Debug"
}

if (!$ApplicationPackagePath)
{
    $ApplicationPackagePath = "pkg\$Configuration"
}

function CleanServiceFabricProject
{
    Param (
        [String]
        $RootPath
    )

    dotnet clean $RootPath --configuration $Configuration
}

Remove-Item -ErrorAction SilentlyContinue -Path GameApplication\$ApplicationPackagePath

foreach ($project in $ServiceFabricProjects)
{
    CleanServiceFabricProject -RootPath $project
}

dotnet clean --configuration $Configuration