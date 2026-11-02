using System.Threading.Channels;
using Core;
using Orchestrator.Settings;
using Orchestrator.Utils;
using Orchestrator.ProcessTools;



SettingsFinder.TryFindStartup(out var stSet);

Log.Info($"Init Port {stSet.StartPort}", "Start Info");
Log.Info($"Port Count {stSet.PortCount}", "Start Info");
Log.Info($"Memory Drop? {stSet.MemoryLeakDrop}", "Start Info");
Log.Info($"Memory Drop Value {stSet.MemoryDropValue} mb", "Start Info");
Log.Info($"Packet For Push {stSet.PacketsForPush}", "Start Info");

PortManager portManager = new PortManager(stSet.PortCount, stSet.StartPort);

ProcessWorker processWorker = new ProcessWorker();




Console.ReadKey();



