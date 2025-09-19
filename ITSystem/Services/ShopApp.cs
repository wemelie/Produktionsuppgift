
using ITSystem.Data;
using ITSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSystem.Services
{
    internal class ShopApp
    {
        private readonly ShopDbContext dbContext;

        public ShopApp(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
       
        internal void Init()
        {
            dbContext.Database.Migrate();

           
            var wanted = new List<Product>
    {
         new Product { Name = "Laptop", Description = "Gaming Laptop", Price = 7000 },
                  new Product { Name = "Iphone 17", Description = "Smartphone", Price = 12000 },
                  new Product { Name = "Gaming chair", Description = "Comfortable", Price = 2000 },
                  new Product { Name = "Laptop 13.3 inch ", Description = "Good", Price = 15000 },
                  new Product { Name = "Samsung galaxy watch", Description = "smart watch", Price = 3500 },
                  new Product { Name = "JBL Speaker", Description = "Portable Speaker", Price = 900 },
                  new Product { Name = "Keyboard", Description = "Keyboard with RGB", Price = 1200 },
                  new Product { Name = "Computer mouse", Description = "Noise free", Price = 500 },
                  new Product { Name = "LG QNED88", Description = "55 INCH TV", Price = 6000 },
                  new Product { Name = "Jabra headphone", Description = "noise cancelling", Price = 1500 } //produkter till databasen
    };

            
            foreach (var p in wanted)
            {
                if (!dbContext.Products.Any(x => x.Name == p.Name))
                    dbContext.Products.Add(p);
            }

            dbContext.SaveChanges();
        }


        internal void RunMenu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== IT-system: Orderhantering ===");
                Console.WriteLine("1) Lista produkter");
                Console.WriteLine("2) Skapa ny order");
                Console.WriteLine("3) Lista ordrar");
                Console.WriteLine("0) Avsluta");
                Console.Write("Välj: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ListProducts(); break;
                    case "2": CreateOrder(); break;
                    case "3": ListOrders(); break;
                    case "0": return;
                    default: Console.WriteLine("Ogiltigt val."); break;
                }
            }
        }

        private void ListProducts()
        {
            Console.WriteLine("\n— Produkter —");
            foreach (var p in dbContext.Products.OrderBy(p => p.Id))
                Console.WriteLine($"Id:{p.Id} | {p.Name} | {p.Description} | {p.Price} kr");
        }

        private void CreateOrder()
        {
            Console.Write("\nKundens namn: ");
            var customer = Console.ReadLine() ?? "";
            var order = new Order { CustomerName = customer };

            while (true)
            {
                ListProducts();

                Console.Write("Produkt-Id (ENTER för klar): ");
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) break;

                if (!int.TryParse(s, out var pid)) { Console.WriteLine("Fel Id."); continue; }
                var product = dbContext.Products.FirstOrDefault(p => p.Id == pid);
                if (product == null) { Console.WriteLine("Produkt finns ej."); continue; }

                Console.Write("Antal: ");
                if (!int.TryParse(Console.ReadLine(), out var qty) || qty <= 0)
                { Console.WriteLine("Fel antal."); continue; }

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = qty,
                    UnitPrice = product.Price
                });

                Console.WriteLine($"+ {qty} x {product.Name} tillagd.");
            }

            if (order.Items.Count == 0) { Console.WriteLine("Ingen rad lades till."); return; }

            dbContext.Orders.Add(order);
            dbContext.SaveChanges();
            Console.WriteLine($"Order #{order.Id} skapad med {order.Items.Count} rader.");
        }

        private void ListOrders()
        {
            Console.WriteLine("\n— Ordrar —");

            var orders = dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Id)
                .ToList();

            if (!orders.Any()) { Console.WriteLine("Inga ordrar."); return; }

            foreach (var o in orders)
            {
                Console.WriteLine($"Order #{o.Id} | {o.CreatedAt:g} | Kund: {o.CustomerName}");
                decimal total = 0;
                foreach (var i in o.Items)
                {
                    var name = i.Product?.Name ?? $"ProductId {i.ProductId}";
                    var row = i.UnitPrice * i.Quantity;
                    total += row;
                    Console.WriteLine($"   - {i.Quantity} x {name} , {i.UnitPrice} kr = {row} kr");
                }
                Console.WriteLine($"   Totalt: {total} kr");
            }
        }
    }
}
