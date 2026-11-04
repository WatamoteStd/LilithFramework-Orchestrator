using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Orchestrator.Core;
using Orchestrator.ProcessTools.Memory;
using Orchestrator.Utils;

namespace Orchestrator.ProcessTools;

/// <summary>
/// Unsafe class using for maximalize analyzer speed.
/// </summary>
public unsafe class MemoryAnalyzer : IDisposable
{

    public ushort MemoryDropValue {get; private set;}
    public bool CanDrop { get; private set; } = false;
    private readonly MemoryAnalyzerMods _workMode = MemoryAnalyzerMods.Balanced;
    private readonly ushort _portsCount;
    private readonly ushort _startPort;
    private readonly ushort _perPortMemoryBytes;

    // UNSAFE
    private readonly byte* _memoryPtr;
    private readonly ushort PER_PORT_SNAPSHOT_COUNT; 
    private readonly ushort[] _writeIndices;
    private readonly uint _totalMemory;

    public MemoryAnalyzer(ushort portsCount, ushort memoryValue, bool canDrop, MemoryAnalyzerMods workMode, ushort perPortMemory, ushort startPort)
    {
        
        _portsCount = portsCount;
        MemoryDropValue = memoryValue;
        CanDrop = canDrop;
        _workMode = workMode;
        _perPortMemoryBytes = perPortMemory;
        _startPort = startPort;

        _writeIndices = new ushort[portsCount];
        _totalMemory = (uint)(portsCount * perPortMemory);

        PER_PORT_SNAPSHOT_COUNT = (ushort)(_perPortMemoryBytes / Unsafe.SizeOf<MemorySnapshot>());

        _memoryPtr = (byte*)NativeMemory.AllocZeroed((nuint)_totalMemory);

    }

    /// <summary>
    /// Write memory snapshot to the port buffer.
    /// </summary>
    /// <param name="port">Port number (not index)</param>
    /// <param name="snapshot">Memory snapshot (MB)</param>
    private void Write(ushort port, MemorySnapshot snapshot)
    {
        ushort portIndex = (ushort)(port - _startPort);

        if(portIndex >= _portsCount) return;
        if (_writeIndices[portIndex] >= PER_PORT_SNAPSHOT_COUNT)
        {
            Log.Warning($"Buffer full -> PORT:{port}. Snapshot was droped.", "MemoryAnalyzer");
            return;
        }

        byte* portBasePtr = _memoryPtr + (portIndex * _perPortMemoryBytes);
        MemorySnapshot* target = (MemorySnapshot*)portBasePtr + _writeIndices[portIndex];
        *target = snapshot;
        _writeIndices[portIndex]++;

    }

    private void Clear(ushort port)
    {
        
        ushort portIndex = (ushort)(port - _startPort);
        
        if (portIndex >= _portsCount) return;
        _writeIndices[portIndex] = 0;

    }



    public void Dispose()
    {
        
        if(_memoryPtr != null)
        {
            NativeMemory.Free(_memoryPtr);
        }

    }

    

}
