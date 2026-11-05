using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using Orchestrator.Core;
using Orchestrator.Settings;
using Orchestrator.Utils;

namespace Orchestrator.ProcessTools;

public class ProcessWorker
{

    public enum LaunchMode : byte
    {
        Hiden = 0
    }

    private readonly ConcurrentDictionary<ushort, Process> _portToProcess = new();
    private readonly FrozenDictionary<InstanceTypes, string> _instanceToPath = FrozenDictionary<InstanceTypes, string>.Empty;


    public ProcessWorker(Dictionary<InstanceTypes, string> instancesDic, bool showCheck = true)
    {
        
        if(instancesDic.Count == 0)
        {
            Log.Error("Can't find any executable file.", "Process Worker");
            return;
        }
        CheckAvaliblity(instancesDic, showCheck);
        _instanceToPath = instancesDic.ToFrozenDictionary();

    }
    public ProcessWorker(bool showCheck = true) : this(SettingsFinder.FindDefaulthInstancesPaths(), showCheck)
    {
        
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
    public IEnumerable<KeyValuePair<ushort, Process>> GetActiveProcesses() => _portToProcess;

    private bool CheckAvaliblity(InstanceTypes instance) => _instanceToPath.ContainsKey(instance);

    public void ExecuteInstance(InstanceTypes instanceType, LaunchMode mode, ushort port)
    {
        
        if(!CheckAvaliblity(instanceType))
        {
            Log.Error($"Can't start process for instance:{instanceType.ToString()}", "ProcessWorker | ExecuteInstance");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.FileName = _instanceToPath[instanceType];
        startInfo.WorkingDirectory = Path.GetDirectoryName(_instanceToPath[instanceType]);
        startInfo.Environment["SERVER_PORT"] = port.ToString();

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        var process = Process.Start(startInfo);
        if (process == null) return;

        process.EnableRaisingEvents = true;
        _portToProcess[port] = process;

        process.Exited += (sender, args) =>
        {
            
            _portToProcess.TryRemove(port, out _);
            Log.Success($"Closed instance at PORT: {port}", "ProcessWorker | Exited");

        };

        process.OutputDataReceived += (sender, e) =>
        {
            if(!string.IsNullOrEmpty(e.Data))
                Log.ServerOutput(e.Data, $"PORT:{port}");
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if(!string.IsNullOrEmpty(e.Data))
                Log.ServerOutput(e.Data, $"PORT:{port}");
        };

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        Log.Success($"Started new instance at port: {port}", "ProcessWorker | Execute");


    }

    public void KillProcess(ushort port)
    {
        
        if(_portToProcess.TryGetValue(port, out var process))
        {
            process.Kill(true);
        }

    }

}