using Orchestrator.Core;
using Orchestrator.Settings;
using Orchestrator.Utils;
using Orchestrator.ProcessTools;
using Orchestrator.ProcessTools.Memory;



SettingsFinder.TryFindStartup(out var stSet);
var instancesPathsDic = SettingsFinder.FindDefaulthInstancesPaths();

foreach(var pair in instancesPathsDic)
{

    byte attemps = 0;

    bool pathValid = File.Exists(pair.Value) ? true : false;
    if (pathValid) continue;

    Log.Dialog($"Path for '{pair.Key.ToString()}' does not exists. Wanna enter to manual mode? (Y/N)", "System");
    string userInput = Console.ReadLine() ?? "N";
    userInput = userInput.ToLower();

    if(userInput == "n") continue;
    
    Log.Dialog("Enter absolute to executable file below:", "System");

    while(attemps < 3)
    {
        
        string? rawPath = Console.ReadLine();

        if(!string.IsNullOrWhiteSpace(rawPath))
        {
            if(!File.Exists(rawPath) && attemps < 2)
            {
                attemps++;
                Log.Dialog("Can't find any executable file in this directory. Try again.", "System");
            }
            else if(!File.Exists(rawPath) && attemps >= 2)
            {
                Console.Clear();
                break;
            }
            else
            {
                instancesPathsDic[pair.Key] = rawPath;
                SettingsFinder.SaveMobaPath(rawPath);
                break;
            }
        } 
        else
        {
            attemps++;
        }  

    }

    

}

Log.Info($"Init Port {stSet.StartPort}", "Start Info");
Log.Info($"Port Count {stSet.PortCount}", "Start Info");
Log.Info($"Memory Drop? {stSet.MemoryLeakDrop}", "Start Info");
Log.Info($"Memory Drop Value {stSet.MemoryDropValue} mb", "Start Info");
Log.Info($"Packet For Push {stSet.PacketsForPush}", "Start Info");

PortManager portManager = new PortManager(stSet.PortCount, stSet.StartPort);
ProcessWorker processWorker = new ProcessWorker();

portManager.TryGetPort(out ushort port);
processWorker.ExecuteInstance(InstanceTypes.Moba, ProcessWorker.LaunchMode.Windowed, port);

portManager.TryGetPort(out ushort port2);
processWorker.ExecuteInstance(InstanceTypes.Moba, ProcessWorker.LaunchMode.Windowed, port2);

var cts = new CancellationTokenSource();

using(MemoryAnalyzer memoryAnalyzer = new MemoryAnalyzer(stSet.PortCount, stSet.MemoryDropValue,
    stSet.MemoryLeakDrop, MemoryAnalyzerMods.Balanced, 64000, stSet.StartPort))
{
    
    memoryAnalyzer.OnPortProcessKilled += (port) =>
    {
        
        portManager.ReleasePort(port);
        processWorker.KillProcess(port);

    };

    _ = Task.Run(async () =>
    {
        
        while(!cts.Token.IsCancellationRequested)
        {
        
            memoryAnalyzer.AnalyzeInstances(processWorker.GetActiveProcesses());
            await Task.Delay(2000, cts.Token);

        }


    });

    Console.ReadKey();
    cts.Cancel();

}



Console.ReadKey();



