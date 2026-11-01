using System.Threading.Channels;
using Core;
using Orchestrator.Settings;
using Orchestrator.Utils;
using ProcessTools;



SettingsFinder.TryFindStartup(out var stSet);

Log.Info($"Init Port {stSet.StartPort}", "Start Info");
Log.Info($"Port Count {stSet.PortCount}", "Start Info");
Log.Info($"Memory Drop? {stSet.MemoryLeakDrop}", "Start Info");
Log.Info($"Memory Drop Value {stSet.MemoryDropValue} mb", "Start Info");
Log.Info($"Packet For Push {stSet.PacketsForPush}", "Start Info");

PortManager portManager = new PortManager(stSet.PortCount, stSet.StartPort);

ProcessWorker processWorker = new ProcessWorker();


_ = Task.Run(async () =>
{
    
    while(true)
    {
        
        await Task.Delay(5000);

        if(portManager.TryGetPort(out ushort port))
        {
            try
            {
                processWorker.ExecuteInstance(InstanceTypes.Moba, ProcessWorker.LaunchMode.Hiden, port);
            }
            catch (Exception e)
            {
                Log.Warning($"Can't execute instance. Error:{e.Message}", "Process Worker");
                portManager.ReleasePort(port);
            }
        }
        else
        {
            Log.Error($"No free ports, can't execute new instances.", "Port Manager");
            portManager.ShowPortInfo(false);

        }

    }

});


Console.ReadKey();



