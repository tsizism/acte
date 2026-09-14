-- sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "BACKUP DATABASE HoldingsDb TO DISK='Y:\LossLess\7.swProjects\acte\webapp\Balazor\UIPoc\acte\UIPoc\DatabaseBackup\HoldingsDb.bak' WITH FORMAT, INIT"

BACKUP DATABASE HoldingsDB
TO DISK = 'C:\Backups\HoldingsDB.bak'
WITH FORMAT, MEDIANAME = 'LocalDB_Backups', NAME = 'Full Backup of HoldingsDB';