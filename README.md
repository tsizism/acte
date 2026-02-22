Your Program.cs already has await dbContext.Database.MigrateAsync() in the InitializeDatabaseAsync(WebApplication) method, 
so the migration will be automatically applied when you run your Blazor application.
Alternatively, you can manually apply the migration now using:


dotnet ef database update

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