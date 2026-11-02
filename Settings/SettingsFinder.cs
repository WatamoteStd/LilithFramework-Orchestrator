
using System.Text.Json;
using Orchestrator.Core;
using Orchestrator.Utils;

namespace Orchestrator.Settings;

public static class SettingsFinder
{

    private static readonly StartupSettings _basicStartupSettings = new StartupSettings(15, 28765, true, 177, 10);

    public static bool TryFindStartup(out StartupSettings stSet)
    {
        stSet = _basicStartupSettings;

        var filePath = FindConfigFilePath("orchestrator.json");

        if(filePath == null)
        {
            Log.Warning("Not found config file, using base params.", "SettingsFinder | Startup");
            return false;
        }

        var text = File.ReadAllText(filePath);
        Log.Info("Found settings file.", "SettingFinder| Startup");
        
        try
        {
            var parsed = JsonSerializer.Deserialize<StartupSettings>(text);
            stSet = parsed;
            return true;
        }
        catch(JsonException e)
        {
            Log.Error($"Failed to parse config: {e.Message}", "SettingsFinder | Startup");
            return false;
        }

    }

    public static Dictionary<InstanceTypes, string> FindDefaulthInstancesPaths()
    {

        Dictionary<InstanceTypes, string> origin = new Dictionary<InstanceTypes, string>();

        var filePath = FindConfigFilePath("orchestrator_instances.json");
        if(filePath == null)
        {
            Log.Error("Not found config file, can't execute any instance if use no reserve root.", "SettingsFinder | InsatancePaths");
            return origin;
        }

        var text = File.ReadAllText(filePath);

        try
        {
            
            var parsed = JsonSerializer.Deserialize<InstancesPaths>(text);
            origin.Add(InstanceTypes.Moba, parsed.MobaServer);

        }
        catch(JsonException e)
        {
            
            Log.Error($"Failed to parse config: {e.Message}", "SettingsFinder | Startup");
            return origin;

        }
        return origin;

    }

    public static void SaveMobaPath(string path)
    {
        
        var file = FindConfigFilePath("orchestrator_instances.json");
        if(file is null)
        {
            Log.Error($"Can't write new path to config, invalid configuration file", "Error");
            return;
        }
        InstancesPaths newPaths = new InstancesPaths { MobaServer = path };
        var json = JsonSerializer.Serialize(newPaths, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(file, json);

    }



    // FOR DIRECTORY SEARCH
    private static string? FindProjectRoot()
    {
        
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while(dir != null)
        {
            
            if(Directory.Exists(Path.Combine(dir.FullName, "configs"))) return dir.FullName;

            dir = dir.Parent;

        }

        return null;

    }

    private static string? FindConfigFilePath(string fileName)
    {
        
        var root = FindProjectRoot();
        if(root == null) return null;

        var configDir = Path.Combine(root, "configs", "editor");
        var fullPath = Path.Combine(configDir, fileName);

        return File.Exists(fullPath) ? fullPath : null;

    }

}
