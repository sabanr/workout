# ================================
# Configuration
# ================================
$sqlite = "sqlite3.exe"

$source = Join-Path $env:USERPROFILE "AppData\Local\User Name\dev.rsaban.irontracker\Data\irontracker.db"

if ([string]::IsNullOrWhiteSpace($env:OneDriveConsumer)) {
    throw "OneDriveConsumer is not set. If you use OneDrive for Business, use `$env:OneDrive instead."
}
$backupFolder = Join-Path $env:OneDriveConsumer "IronTracker\Backups"

# ================================
# Pre-flight checks
# ================================
if (!(Get-Command $sqlite -ErrorAction SilentlyContinue)) {
    throw "sqlite3.exe not found in PATH. Put sqlite3.exe in PATH or set `$sqlite to its full path."
}
if (!(Test-Path -LiteralPath $source)) { throw "Source DB not found: $source" }

# Ensure backup folder exists
if (!(Test-Path -LiteralPath $backupFolder)) {
    New-Item -ItemType Directory -Path $backupFolder | Out-Null
}

# Create destination filename and resolve to full path
$date = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$destination = Join-Path $backupFolder "database_$date.db"
$destinationFull = (Resolve-Path -LiteralPath $backupFolder).Path + "\" + (Split-Path -Leaf $destination)

# ================================
# Perform safe SQLite backup
# ================================
# IMPORTANT: use single quotes for sqlite meta-command argument
# Also escape any single quotes in path (rare on Windows, but correct)
$destForSqlite = $destinationFull -replace "'", "''"
$cmd = ".backup '$destForSqlite'"

$backupOutput = & $sqlite $source $cmd 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "SQLite backup failed (exit $LASTEXITCODE). Output: $backupOutput"
}

# Verify backup size
$dstInfo = Get-Item -LiteralPath $destinationFull -ErrorAction Stop
if ($dstInfo.Length -le 0) {
    throw "Backup was created but is 0 bytes: $destinationFull`nSQLite output: $backupOutput"
}

# Delete backups older than 30 days
Get-ChildItem -LiteralPath $backupFolder -Filter "database_*.db" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force
