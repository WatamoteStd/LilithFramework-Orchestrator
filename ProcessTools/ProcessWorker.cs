using System.Collections.Frozen;
using System.Diagnostics;
using Core;
using Orchestrator.Settings;
using Orchestrator.Utils;

namespace ProcessTools;

public class ProcessWorker
{

    public enum LaunchMode : byte
    {
        Windowed = 0,
        Hiden = 1

    }

    private readonly Dictionary<ushort, Process> _portToProcess = new();
    private readonly FrozenDictionary<InstanceTypes, string> _instanceToPath = FrozenDictionary<InstanceTypes, string>.Empty;


    public ProcessWorker(bool showCheck = true)
    {
        
        var originDic = SettingsFinder.FindDefaulthInstancesPaths();
        if(originDic.Count == 0)
        {
            Log.Error("Can't find any executable file.", "Process Worker");
            return;
        }
        CheckAvaliblity(originDic, showCheck);
        _instanceToPath = originDic.ToFrozenDictionary();

    }

    private void CheckAvaliblity(Dictionary<InstanceTypes, string> originDic, bool showCheck)
    {

        List<InstanceTypes> removeList = new();

        
        foreach(var pair in originDic)
        {
            
            if(!File.Exists(pair.Value))
            {
                removeList.Add(pair.Key);
            }

        }

        for(int i = 0; i < removeList.Count; i++)
        {
            originDic.Remove(removeList[i]);
        }

        if(showCheck)
        {
            
            if(removeList.Count > 0)
            {
                Log.Warning($"Invalid executable files path for: {removeList.Count} instances", "ProcessWorker | Paths Check");            
                return;
            }
            Log.Success("All executable files is valid.", "Process Worker | File Check");

        }


    }

    public void ExecuteInstance(InstanceTypes instanceType, LaunchMode mode, ushort port)
    {
        
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = _instanceToPath[instanceType];
        startInfo.WorkingDirectory = Path.GetDirectoryName(_instanceToPath[instanceType]);
        startInfo.Environment["SERVER_PORT"] = port.ToString();

        switch(mode)
        {
            
            case LaunchMode.Windowed:
                {
                    startInfo.CreateNoWindow = false;
                }
            break;

            case LaunchMode.Hiden:
                {
                    startInfo.CreateNoWindow = true;
                }
            break;

        }

        var process = Process.Start(startInfo);
        if (process == null) return;

        process.EnableRaisingEvents = true;
        _portToProcess[port] = process;

        process.Exited += (sender, args) =>
        {
            
            _portToProcess.Remove(port);


        };

        Log.Success($"Started new instance at port: {port}", "ProcessWorker | Execute");


    }

}