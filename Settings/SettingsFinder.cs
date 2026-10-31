
using System.Text.Json;

namespace Orchestrator.Settings;

public static class SettingsFinder
{

    private static readonly StartupSettings _basicStartupSettings = new StartupSettings(15, 28765, false, 0);
    
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
            Console.WriteLine($"[SettingFinder| Startup] Not found config file, using base params.");
            return false;
        }

        var text = File.ReadAllText(filePath);
        Console.WriteLine($"[SettingFinder| Startup] Finded settings file.");
        stSet = JsonSerializer.Deserialize<StartupSettings>(text);
        return true;
        

    }

}