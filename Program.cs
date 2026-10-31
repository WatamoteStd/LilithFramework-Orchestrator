using System.Threading.Channels;
using Core;




PortManager portManager = new PortManager(15, 30000);


Console.WriteLine($"================= ORCHESTRATOR STARTET =================");
portManager.ShowPortInfo();

