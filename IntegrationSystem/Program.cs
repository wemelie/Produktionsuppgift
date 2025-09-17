
using System;
using System.IO;
using System.Linq;
using System.Threading;
using EasyModbus;                      // EasyModbusTCP.NET
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ITSystem.Data;                   // ShopDbContext (från ITSystem-projektet)
using ITSystem.Models;                 // Order


namespace IntegrationSystem
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // 1) Läs konfiguration
                var cfg = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connStr = cfg.GetConnectionString("DefaultConnection");
                var host = cfg["Modbus:Host"] ?? "127.0.0.1";
                var port = int.TryParse(cfg["Modbus:Port"], out var p) ? p : 502;
                var reg = int.TryParse(cfg["Modbus:RegisterAddress"], out var r) ? r : 0;
                var unitId = byte.TryParse(cfg["Modbus:UnitId"], out var u) ? u : (byte)1;

                var pollMs = int.TryParse(cfg["Integration:PollIntervalMs"], out var pm) ? pm : 1000;
                var delayMs = int.TryParse(cfg["Integration:DelayMsBetweenWrites"], out var d) ? d : 200;
                var batch = int.TryParse(cfg["Integration:BatchSize"], out var b) ? b : 100;
                var stateFile = cfg["Integration:StateFile"] ?? "last.txt";

                // 2) Förbered DB options
                var dbOptions = new DbContextOptionsBuilder<ShopDbContext>()
                    .UseSqlServer(connStr)
                    .Options;

                // 3) Förbered Modbus-klient
                var client = new ModbusClient(host, port) { UnitIdentifier = unitId };

                // Ctrl+C stänger snyggt
                var stopping = false;
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    stopping = true;
                    Console.WriteLine("Avslutar... (Ctrl+C)");
                };

                // Hämta senast skickade orderId från state-fil
                int last = ReadLast(stateFile);

                Console.WriteLine($"Integration startad. Lyssnar på nya ordrar > {last}. " +
                                  $"Modbus: {host}:{port} HR[{reg}]  (Poll={pollMs}ms)");

                while (!stopping)
                {
                    // 4) Se till att vi är anslutna (retry om ot-systemet inte hunnit upp)
                    if (!client.Connected)
                    {
                        try
                        {
                            client.Connect();
                            Console.WriteLine($"[Modbus] Ansluten till {host}:{port}");
                        }
                        catch
                        {
                            Console.WriteLine("[Modbus] Kunde inte ansluta. Försöker igen om 1s...");
                            Thread.Sleep(1000);
                            continue;
                        }
                    }

                    try
                    {
                        // 5) Läs NYA ordrar (> last) och skicka
                        using var db = new ShopDbContext(dbOptions);
                        var newOrders = db.Orders
                            .AsNoTracking()
                            .Where(o => o.Id > last)
                            .OrderBy(o => o.Id)
                            .Take(batch)
                            .ToList();

                        if (newOrders.Count == 0)
                        {
                            // Inga nya — vänta och loopa igen
                            Thread.Sleep(pollMs);
                            continue;
                        }

                        foreach (var o in newOrders)
                        {
                            int value = o.Id & 0xFFFF; // holding register är 16-bit
                            client.WriteSingleRegister(reg, value);
                            Console.WriteLine($"Order har skapats för {o.CustomerName} med order Id {o.Id}. Skickas till OT → HR[{reg}].");

                            last = o.Id;
                            SaveLast(stateFile, last);
                            Thread.Sleep(delayMs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Fel] {ex.Message}");
                        // Om anslutning dött, stäng och försök koppla upp igen
                        if (client.Connected)
                        {
                            try { client.Disconnect(); } catch { /* ignore */ }
                        }
                        Thread.Sleep(1000);
                    }
                }

                // Stäng vid avslut
                if (client.Connected) client.Disconnect();
                Console.WriteLine("Integration stoppad.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        static int ReadLast(string path)
        {
            try
            {
                if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out var n))
                    return n;
            }
            catch { /* ignore */ }
            return 0;
        }

        static void SaveLast(string path, int n)
        {
            try { File.WriteAllText(path, n.ToString()); }
            catch { /* ignore */ }
        }
    }
}
