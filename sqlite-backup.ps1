# ================================
# Configuration
# ================================
$source = "~\AppData\Local\User Name\dev.rsaban.irontracker\Data\irontracker.db"
$sqlite = "sqlite3.exe"
$backupFolder = "$env:OneDriveConsumer\IronTracker\Backups"

# ================================
# Ensure backup folder exists
# ================================
if (!(Test-Path $backupFolder)) {
    New-Item -ItemType Directory -Path $backupFolder | Out-Null
}

# ================================
# Create date-based filename
# ================================
$date = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$destination = Join-Path $backupFolder "database_$date.db"

# ================================
# Perform safe SQLite backup
# ================================
& $sqlite $source ".backup '$destination'"

# ================================
# Delete backups older than 30 days
# ================================
Get-ChildItem $backupFolder -Filter "database_*.db" |
Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
Remove-Item -Force


