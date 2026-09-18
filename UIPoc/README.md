Your Program.cs already has await dbContext.Database.MigrateAsync() in the InitializeDatabaseAsync(WebApplication) method, 
so the migration will be automatically applied when you run your Blazor application.
Alternatively, you can manually apply the migration now using:


PS Y:\LossLess\7.swProjects\acte\webapp\Balazor\UIPoc\acte\UIPoc> 
dotnet tool update --global dotnet-ef
Tool 'dotnet-ef' was successfully updated from version '10.0.3' to version '10.0.12'.

1. class Equity --> Add public decimal AverageCostMarket { get; set; } to 
2. class HoldingsDbContext --> Add entity.Property(e => e.AverageCostMarket).IsRequired().HasColumnType("decimal(18,2)"); 
to the OnModelCreating method in your DbContext class (e.g., HoldingsDbContext).
3. In the Package Manager Console (PMC), run the following command to create a new migration:
---> dotnet ef migrations add AverageCostMarket_ToEquity
4. In the PMC, run the following command to apply the migration and update the database:
---> dotnet ef database update
5. Copy and verify the changes in your database using SQL Server Management Studio (SSMS) or any other database management tool of your choice.
UPDATE Equities
SET AverageCostMarket = AverageCost

SELECT EquityId, AverageCost, AverageCostMarket
FROM Equities



The database and tables will be created in your SQL Express instance as configured in your connection string!
sqllocaldb info mssqllocaldb


To Verify the Database Was Created:

# Connect to LocalDB
sqlcmd -S "(localdb)\mssqllocaldb"

# List databases (in sqlcmd)
SELECT name FROM sys.databases;
GO

sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb


Next Steps: in PMC Update-Database
The migration will be automatically applied when you run your Blazor app (because of the MigrateAsync() call in Program.cs).
Or apply it manually now: dotnet ef database update


git config --global credential.helper store
git push origin main
tsizism
token



#sqlcmd -S (localdb)\MSSQLLocalDB -Q "DROP DATABASE HoldingsDb"
#sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE HoldingsDb FROM DISK='C:\Users\tsizi\localdb\HoldingsDB.bak' WITH RECOVERY, REPLACE"
#sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE HoldingsDbBackup FROM DISK='C:\Users\tsizi\localdb\HoldingsDB.bak' WITH REPLACE, MOVE 'Logical_Data_Name' TO 'C:\Users\tsizi\localdb\HoldingsDB2.bak', MOVE 'Logical_Log_Name' TO 'C:\Users\tsizi\localdb\HoldingsDB2.log.ldf'"



sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "BACKUP DATABASE HoldingsDb TO DISK='Y:\LossLess\7.swProjects\acte\webapp\Balazor\UIPoc\acte\UIPoc\DatabaseBackup\HoldingsDb.bak' WITH FORMAT, INIT"


Command Breakdown:
-S "(localdb)\MSSQLLocalDB": Connects directly to your default LocalDB instance.
-E: Uses integrated Windows Authentication to sign in.
-Q: Executes the SQL query inside the quotation marks and exits immediately.
WITH FORMAT, INIT: Overwrites any existing backup file at that location and creates a clean, new backup set.


#sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE HoldingsDb FROM DISK='Y:\LossLess\7.swProjects\acte\webapp\Balazor\UIPoc\acte\UIPoc\Database\HoldingsDb.bak' WITH RECOVERY, REPLACE"