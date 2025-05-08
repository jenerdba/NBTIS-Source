<!---- Run this command in Package Manager Console to update Models from SQL Server on-premise(Note this will have default project and the project where the command has to run) ----->

Scaffold-DbContext "Server=mssql-d;Database=NBI;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -Context DataContext -OutputDir Models -Schemas "NBI" -f -NoOnConfiguring -Project NBTIS.Data -StartupProject NBTIS.Web

<!---- Run this command to update DBContext from Azure SQL ------->

 Scaffold-DbContext -Connection "Server=tcp:sqlsvr-fhwa-test-01.d601c4ea25fd.database.windows.net,1433;Database=NBTIS-Dev-DB;Authentication=Active Directory Integrated;Encrypt=True;" -Provider Microsoft.EntityFrameworkCore.SqlServer -Context DataContext -OutputDir Models -Schemas NBI -Project NBTIS.Data -StartupProject NBTIS.Web -NoOnConfiguring  -UseDatabaseNames -Force