#!/bin/zsh

BACKUP_DIR="$HOME/Library/CloudStorage/OneDrive-Personal/IronTracker/Backups"
DB_PATH="$HOME/Library/Containers/dev.rsaban.irontracker/Data/Library/irontracker.db"

LATEST_BACKUP=$(ls -t "$BACKUP_DIR"/database_*.db 2>/dev/null | head -1)

if [[ -z "$LATEST_BACKUP" ]]; then
    echo "Error: No backup files found in $BACKUP_DIR"
    exit 1
fi

echo "Latest backup: $(basename "$LATEST_BACKUP")"
echo "Restoring to: $DB_PATH"

rm -f "$DB_PATH" "$DB_PATH-shm" "$DB_PATH-wal"

sqlite3 "$DB_PATH" ".restore '$LATEST_BACKUP'"

echo "Database restored successfully!"
