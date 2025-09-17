
using System;
using System.IO;
using System.Linq;
using System.Threading;
using EasyModbus;                      
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ITSystem.Data;                   
using ITSystem.Models;                 


namespace IntegrationSystem
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                
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

               
                var dbOptions = new DbContextOptionsBuilder<ShopDbContext>()
                    .UseSqlServer(connStr)
                    .Options;

                
                var client = new ModbusClient(host, port) { UnitIdentifier = unitId };

                
                var stopping = false;
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    stopping = true;
                    Console.WriteLine("Avslutar... (Ctrl+C)");
                };

                int last = ReadLast(stateFile);

                Console.WriteLine($"Integration startad. Lyssnar på nya ordrar > {last}. " +
                                  $"Modbus: {host}:{port} HR[{reg}]  (Poll={pollMs}ms)");

                while (!stopping)
                {
                  
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
                            int value = o.Id & 0xFFFF; 
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
