using System.Runtime.InteropServices;

namespace Orchestrator.ProcessTools.Memory;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MemorySnapshot
{
    public readonly ushort MemoryUsageMB;
}