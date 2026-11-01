
using System.Text.Json;
using Core;
using Orchestrator.Utils;

namespace Orchestrator.Settings;

public static class SettingsFinder
{

    private static readonly StartupSettings _basicStartupSettings = new StartupSettings(15, 28765, true, 177, 10);
    
    public static bool TryFindStartup(out StartupSettings stSet)
    {
        stSet = _basicStartupSettings;
        
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while(dir != null && !Directory.Exists(Path.Combine(dir.FullName, "configs")))
        {
            dir = dir.Parent;
        }

        if (dir == null) throw new DirectoryNotFoundException("config folder not found. Check the path.");
        var configDir = Path.Combine(dir.FullName, "configs", "editor");
        var filePath = Path.Combine(configDir, "orchestrator.json");

        if(!File.Exists(filePath))
        {
            Log.Warning("Not found config file, using base params.", "SettingsFinder | Startup");
            return false;
        }

        var text = File.ReadAllText(filePath);
        Log.Info("Finded settings file.", "SettingFinder| Startup");
        stSet = JsonSerializer.Deserialize<StartupSettings>(text);
        return true;
        

    }

    public static Dictionary<InstanceTypes, string> FindDefaulthInstancesPaths()
    {
        
        Dictionary<InstanceTypes, string> origin = new Dictionary<InstanceTypes, string>();

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while(dir != null && !Directory.Exists(Path.Combine(dir.FullName, "configs")))
        {
            dir = dir.Parent;
        }

        if (dir == null) throw new DirectoryNotFoundException("config folder not found. Check the path.");
        var configDir = Path.Combine(dir.FullName, "configs", "editor");
        var filePath = Path.Combine(configDir, "orchestrator_instances.json");

        if(!File.Exists(filePath))
        {
            Log.Error("Not found any instance path.", "Settings Finder");
            return origin;
        }

        var text = File.ReadAllText(filePath);
        var json = JsonSerializer.Deserialize<InstancesPaths>(text);

        origin.Add(InstanceTypes.Moba, json.MobaServer);

        return origin;

    }

}