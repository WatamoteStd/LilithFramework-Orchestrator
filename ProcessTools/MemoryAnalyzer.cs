

using Orchestrator.Core;

namespace Orchestrator.ProcessTools;

public static class MemoryAnalyzer
{

    public static ushort MemoryDropValue {get; private set;}
    public static bool CanDrop { get; private set; } = false;
    public static MemoryAnalyzerMode AnalyzeMode {get; private set;}

    public static void InitDefault(ushort memoryValue, bool canDrop)
    {

        MemoryDropValue = memoryValue;
        CanDrop = canDrop;

    }

    public static void AnalyzeInstances()
    {



    }

}
