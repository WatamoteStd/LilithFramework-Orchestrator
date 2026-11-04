
using System.Diagnostics;

namespace Orchestrator.ProcessTools.Memory;

public interface IMemoryAnalyzer
{
    
    void AnalyzeInstances(IEnumerable<KeyValuePair<ushort, Process>> activeProcess);
    event Action<ushort> OnPortProcessKilled;

}