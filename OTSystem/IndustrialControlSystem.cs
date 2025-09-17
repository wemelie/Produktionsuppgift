using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTSystem
{
    internal class IndustrialControlSystem
    {
        private static double currentTemperature = 20.0;
        private const double TargetTemperature = 25.0;
        private static bool heaterOn = false;
        private static bool messageReceived = false;

        public void Run()
        {
            Console.WriteLine("Simulated OT system with Modbus support");

            Thread modbusThread = new Thread(StartEasyModbusTcpSlave);
            modbusThread.IsBackground = true;
            modbusThread.Start();

            while (true)
            {
              

                if (messageReceived)
                {
                    Console.WriteLine("Order received via Modbus!");
                    messageReceived = false;
                }

                Thread.Sleep(1000);
            }
        }
        public static void StartEasyModbusTcpSlave()
        {
            int port = 502;
           
            ModbusServer modbusServer = new ModbusServer();
            modbusServer.Port = port; 

           
         
            modbusServer.CoilsChanged += (int startAddress, int numberOfCoils) =>
            {
                Console.WriteLine($"CoilsChanged event fired at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Coils: {numberOfCoils}");

              
                const int maxCoilAddress = 1999; 

                for (int i = 0; i < numberOfCoils; i++)
                {
                    int address = startAddress + i;
                   
                    if (address >= 0 && address <= maxCoilAddress) 
                    {
                        Console.WriteLine($"    Coil[{address}] changed to: {modbusServer.coils[address]}");
                    }
                    else
                    {
                        Console.WriteLine($"    Warning: Attempted to access Coil[{address}] which is out of bounds.");
                    }
                }
            };

         
            modbusServer.HoldingRegistersChanged += (int startAddress, int numberOfRegisters) =>
            {
                Console.WriteLine($"HoldingRegistersChanged event fired at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Registers: {numberOfRegisters}");

                
                const int maxRegisterAddress = 1999; 

                for (int i = 0; i < numberOfRegisters; i++)
                {
                    int address = startAddress + i;

                    
                    if (address >= 0 && address <= maxRegisterAddress)
                    {
                      
                        int orderId = modbusServer.holdingRegisters[address];

                        Console.WriteLine($"ordern med Id {orderId} är klar för att skickas ut");

                       
                    }
                    else
                    {
                        Console.WriteLine($"    Warning: Attempted to access HoldingRegister[{address}] which is out of bounds.");
                    }
                }
            };

      
            modbusServer.holdingRegisters[0] = 123;
            modbusServer.holdingRegisters[1] = 456;
            modbusServer.coils[0] = true;
            modbusServer.holdingRegisters[10] = 789; 

            modbusServer.inputRegisters[0] = 999;
            modbusServer.discreteInputs[0] = true;


          
            try
            {
                Console.WriteLine($"Starting EasyModbus TCP Slave on port {port}...");
                modbusServer.Listen();

                Console.WriteLine("EasyModbus TCP Slave started. Press any key to exit.");
                Console.ReadKey(); 

                
                Console.WriteLine("Stopping EasyModbus TCP Slave...");
                Console.WriteLine("EasyModbus TCP Slave stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}