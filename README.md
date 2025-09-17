IT-system: Orderhantering

Ett konsolprogram i C# som hanterar produkter och ordrar i en SQL-databas med hjälp av Entity Framework Core.

Menyval

1. Lista produkter – visar alla produkter i databasen

2. Skapa ny order – lägger till en ny order

3. Lista ordrar – visar alla ordrar

0. Avsluta – stänger programmet

Databas

Products – produkter

Orders – ordrar

OrderItems – rader i en order

När programmet startar fylls databasen automatiskt med 10 produkter.

Kom igång

Ändra connection string i appsettings.json om det behövs.

Kör Update-Database i Package Manager Console för att skapa databasen.

Starta programmet (F5) och använd menyn.
