using System.Threading.Channels;
using Core;
using Orchestrator.Settings;



SettingsFinder.TryFindStartup(out var stSet);

Console.WriteLine($"[Program | StartSettings] INIT PORT:{stSet.StartPort}");
Console.WriteLine($"[Program | StartSettings] PORT COUNT:{stSet.PortCount}");
Console.WriteLine($"[Program | StartSettings] MEMORY DROP?:{stSet.MemoryLeakDrop}");
Console.WriteLine($"[Program | StartSettings] MEMORY DROP VALUE:{stSet.MemoryDropValue}(MB)");




PortManager portManager = new PortManager(stSet.PortCount, stSet.StartPort);


Console.WriteLine($"================= ORCHESTRATOR STARTET =================");
portManager.ShowPortInfo();

