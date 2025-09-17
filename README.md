IT-system: Orderhantering
Detta är ett konsolprogram skrivet i C# som hanterar produkter och ordrar i en SQL Server-databas med hjälp av Entity Framework Core.
Funktioner

När du startar programmet visas en meny:

1. Lista produkter – visar alla produkter i databasen

2. Skapa ny order – lägger till en ny order baserad på produkter

3. Lista ordrar – visar alla ordrar och orderrader

0. Avsluta – stänger programmet

Databas
Programmet använder en SQL-databas som skapas via Entity Framework Core Migrations. Följande tabeller finns:
Products – lagrar produkter
Orders – lagrar orderhuvuden
OrderItems – lagrar orderrader kopplade till ordrar

När programmet körs första gången fylls databasen automatiskt med 10 standardprodukter.

Kom igång:

1.Klona projektet
git clone https://github.com/wemelie/Produktionsuppgift.git

2.Kontrollera connection string
Öppna appsettings.json och se till att connection string fungerar på din dator.
Rekommenderad för Visual Studio (LocalDB):
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MyProduction;Trusted_Connection=True;"

3.Skapa databasen

4.Öppna Package Manager Console i Visual Studio och kör:
Update-Database

5.Starta programmet
Kör projektet (F5). Menyn visas i konsolfönstret.

Tekniker:
C# .NET Console Application
Entity Framework Core
SQL Server / LocalDB
