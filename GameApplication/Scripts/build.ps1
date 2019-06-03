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

function BuildServiceFabricProject
{
    Param (
        [String]
        $RootPath
    )

    dotnet restore $RootPath -s https://api.nuget.org/v3/index.json
    dotnet build $RootPath --configuration $Configuration
}

npm run-script build

foreach ($project in $ServiceFabricProjects)
{
    BuildServiceFabricProject -RootPath $project
}

foreach ($project in $LibraryProjects)
{
    BuildServiceFabricProject -RootPath $project
}

dotnet restore GameApplication -s https://api.nuget.org/v3/index.json
dotnet build GameApplication --configuration $Configuration
