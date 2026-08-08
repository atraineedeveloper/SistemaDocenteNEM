param(
    [Parameter(Mandatory = $true)]
    [string]$IsccPath,

    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$propsPath = Join-Path $repoRoot "Directory.Build.props"
$projectPath = Join-Path $repoRoot "src\SistemaDocente.App.Wpf\SistemaDocente.App.Wpf.csproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\win-x64"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$installerScript = Join-Path $repoRoot "installer\AulaRaiz.iss"

if (-not (Test-Path -LiteralPath $IsccPath -PathType Leaf)) {
    throw "No se encontró ISCC.exe en '$IsccPath'."
}

[xml]$props = Get-Content -LiteralPath $propsPath -Raw
$version = [string]$props.Project.PropertyGroup.VersionPrefix
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Directory.Build.props no contiene VersionPrefix."
}

Remove-Item -LiteralPath $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installerDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishProfile=win-x64-self-contained `
    -p:PublishDir="$publishDir\"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish falló con código $LASTEXITCODE."
}

$requiredPublishFiles = @(
    "SistemaDocente.App.Wpf.exe",
    "coreclr.dll",
    "hostfxr.dll",
    "PresentationFramework.dll",
    "Microsoft.Data.Sqlite.dll"
)
foreach ($fileName in $requiredPublishFiles) {
    $requiredPath = Join-Path $publishDir $fileName
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "La publicación self-contained no contiene '$fileName'."
    }
}

& $IsccPath "/DMyAppVersion=$version" "/DPublishDir=$publishDir" $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "ISCC falló con código $LASTEXITCODE."
}

$expectedInstaller = Join-Path $installerDir "AulaRaiz-Setup-$version-win-x64.exe"
if (-not (Test-Path -LiteralPath $expectedInstaller -PathType Leaf)) {
    throw "No se generó el instalador esperado '$expectedInstaller'."
}

Write-Host "Installer: $expectedInstaller"
