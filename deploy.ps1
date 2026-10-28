<#
.SYNOPSIS
    Publishes IronTracker for Windows and deploys it over the local install.

.DESCRIPTION
    Builds a Release publish, closes any running instance, copies the publish output over the
    install folder, and refreshes the desktop shortcut. Nothing in the install folder is deleted,
    so the WebView2 profile (IronTracker.exe.WebView2) and any other files there survive.

.EXAMPLE
    ./deploy.ps1
    ./deploy.ps1 -SkipBuild -Launch
#>
[CmdletBinding()]
param(
    [string]$InstallDir = 'C:\Program Files\IronTracker',
    [string]$ShortcutName = 'Iron Tracker',
    [switch]$SkipBuild,
    [switch]$Launch
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$framework = 'net10.0-windows10.0.19041.0'
$project = Join-Path $PSScriptRoot 'IronTracker\IronTracker.csproj'
$publishDir = Join-Path $PSScriptRoot "IronTracker\bin\Release\$framework\win-x64\publish"
$exePath = Join-Path $InstallDir 'IronTracker.exe'

if (-not $SkipBuild)
{
    Write-Host 'Publishing IronTracker (Release)...' -ForegroundColor Cyan
    dotnet publish $project -f $framework -c Release
    if ($LASTEXITCODE -ne 0)
    {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

if (-not (Test-Path (Join-Path $publishDir 'IronTracker.exe')))
{
    throw "No publish output found at $publishDir"
}

# Ask a running instance to close; force it only if it doesn't exit in time
$running = @(Get-Process -Name IronTracker -ErrorAction SilentlyContinue)
if ($running.Count -gt 0)
{
    Write-Host 'Closing running IronTracker...' -ForegroundColor Cyan
    $running | ForEach-Object { $null = $_.CloseMainWindow() }
    $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
    Get-Process -Name IronTracker -ErrorAction SilentlyContinue | Stop-Process -Force
}

Write-Host "Deploying to $InstallDir..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null

# Copy only: never purge, so anything else kept in the install folder is left alone
robocopy $publishDir $InstallDir /E /R:2 /W:1 /NFL /NDL /NJH /NP
if ($LASTEXITCODE -ge 8)
{
    throw "robocopy failed with exit code $LASTEXITCODE"
}

# Recreate the shortcut from scratch so Explorer treats it as a new item
$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop "$ShortcutName.lnk"
Remove-Item -LiteralPath $shortcutPath -ErrorAction SilentlyContinue
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $InstallDir
$shortcut.IconLocation = "$exePath,0"
$shortcut.Save()

# Explorer caches icons by path, so tell the shell the exe and shortcut changed
Add-Type -Namespace IronTracker -Name Shell32 -MemberDefinition @'
[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
public static extern void SHChangeNotify(int eventId, uint flags, string item1, IntPtr item2);
'@
$SHCNE_UPDATEITEM = 0x2000
$SHCNE_ASSOCCHANGED = 0x08000000
$SHCNF_PATHW = 0x0005
$SHCNF_FLUSH = 0x1000
foreach ($path in $exePath, $shortcutPath)
{
    [IronTracker.Shell32]::SHChangeNotify($SHCNE_UPDATEITEM, $SHCNF_PATHW -bor $SHCNF_FLUSH, $path, [IntPtr]::Zero)
}
[IronTracker.Shell32]::SHChangeNotify($SHCNE_ASSOCCHANGED, $SHCNF_FLUSH, $null, [IntPtr]::Zero)
& "$env:SystemRoot\System32\ie4uinit.exe" -show

$version = (Get-Item $exePath).VersionInfo.ProductVersion
Write-Host "IronTracker $version deployed to $InstallDir" -ForegroundColor Green

if ($Launch)
{
    Start-Process -FilePath $exePath -WorkingDirectory $InstallDir
}

exit 0
