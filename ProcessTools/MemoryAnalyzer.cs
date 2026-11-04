using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Orchestrator.ProcessTools.Memory;
using Orchestrator.Utils;

namespace Orchestrator.ProcessTools;

/// <summary>
/// Unsafe class using for maximalize analyzer speed.
/// </summary>
public unsafe class MemoryAnalyzer : IDisposable, IMemoryAnalyzer
{

    public event Action<ushort>? OnPortProcessKilled;

    public ushort MemoryDropValue {get; private set;}
    public bool CanDrop { get; private set; } = false;
    private readonly MemoryAnalyzerMods _workMode = MemoryAnalyzerMods.Balanced;
    private readonly ushort _portsCount;
    private readonly ushort _startPort;
    private readonly ushort _perPortMemoryBytes;
    private readonly byte _warmupIterations;

    public const ushort ABSOLUTE_MEMORY_LIMIT_MB = 4096; 

    private readonly object _locker = new();

    // UNSAFE
    private readonly byte* _memoryPtr;
    private readonly ushort PER_PORT_SNAPSHOT_COUNT; 
    private readonly ushort[] _writeIndices;
    private readonly int[] _currentPids; // unique ids for process tracking
    private readonly ushort[] _dynamicLimits; // each port dynamic limit (drop after chang of Pid)
    private readonly byte[] _violations;
    private readonly uint _totalMemory;

    public MemoryAnalyzer(ushort portsCount, ushort memoryValue, bool canDrop, MemoryAnalyzerMods workMode, 
        ushort perPortMemory, ushort startPort, byte warmupInterations = 10)
    {
        
        _portsCount = portsCount;
        MemoryDropValue = memoryValue;
        CanDrop = canDrop;
        _workMode = workMode;
        _perPortMemoryBytes = perPortMemory;
        _startPort = startPort;
        _warmupIterations = warmupInterations;

        _writeIndices = new ushort[portsCount];
        _currentPids = new int[portsCount];
        _dynamicLimits = new ushort[portsCount];
        _violations = new byte[portsCount];
        
        for(int i = 0; i < portsCount; i++)
        {
            _currentPids[i] = -1;
        }


        _totalMemory = (uint)(portsCount * perPortMemory);


        PER_PORT_SNAPSHOT_COUNT = (ushort)(_perPortMemoryBytes / Unsafe.SizeOf<MemorySnapshot>());

        _memoryPtr = (byte*)NativeMemory.AllocZeroed((nuint)_totalMemory);

    }

    /// <summary>
    /// Write memory snapshot to the port buffer.
    /// </summary>
    /// <param name="port">Port number (not index)</param>
    /// <param name="snapshot">Memory snapshot (MB)</param>
    public void Write(ushort port, MemorySnapshot snapshot)
    {
 
        lock(_locker) // rewrite to interlocked
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

    }

    /// <summary>
    /// Defaulth method for only clear
    /// </summary>
    /// <param name="port">Real port, not index</param>
    public void Clear(ushort port)
    {
        
        lock(_locker) // rewrite to interlocked
        {
            
            ushort portIndex = (ushort)(port - _startPort);
        
            if (portIndex >= _portsCount) return;
            _writeIndices[portIndex] = 0;
            _currentPids[portIndex] = -1;
            _dynamicLimits[portIndex] = 0;
            _violations[portIndex] = 0;

        }
       

    }
    /// <summary>
    /// Defaulth method for clear and rewrite.
    /// </summary>
    /// <param name="port">Real port, not index</param>
    /// <param name="processId">Process uniqueId</param>
    public void Clear(ushort port, int processId)
    {
        lock(_locker) // rewrite to interlocked
        {
            
            ushort portIndex = (ushort)(port - _startPort);
        
            if (portIndex >= _portsCount) return;
            _writeIndices[portIndex] = 0;
            _currentPids[portIndex] = processId;
            _dynamicLimits[portIndex] = 0;
            _violations[portIndex] = 0;

        }
    }

    public void AnalyzeInstances(IEnumerable<KeyValuePair<ushort, Process>> activeProcess)
    {
        
        foreach(var pair in activeProcess)
        {
            
            ushort portIndex = (ushort)(pair.Key - _startPort);
            if (portIndex >= _portsCount) continue; 

            if(_currentPids[portIndex] != pair.Value.Id)
            {
                Clear(pair.Key, pair.Value.Id);
            }

            pair.Value.Refresh();
            long rawMemorySize = pair.Value.PrivateMemorySize64 / 1024 / 1024;
            ushort mb = (ushort)Math.Min(rawMemorySize, ushort.MaxValue);

            Write(pair.Key, new MemorySnapshot(mb));
            AnalyzeProcess(portIndex, pair.Key, mb);

        }

    }


    private void AnalyzeProcess(ushort portIndex, ushort port, ushort currentMb)
    {

        if(currentMb > ABSOLUTE_MEMORY_LIMIT_MB)
        {
            Log.Error($"Killing PORT:{port}. Memory {currentMb}MB > Absolute Limit.", "MemoryAnalyzer");
            Clear(port);
            OnPortProcessKilled?.Invoke(port);
            return; 
        }
        

        if(_writeIndices[portIndex] < _warmupIterations) return;

        if(_dynamicLimits[portIndex] == 0)
        {
            
            lock(_locker)
            {

                byte* portBasePtr = _memoryPtr + (portIndex * _perPortMemoryBytes);
                MemorySnapshot* ptr = (MemorySnapshot*)portBasePtr;
                ReadOnlySpan<MemorySnapshot> history = new ReadOnlySpan<MemorySnapshot>(ptr, _warmupIterations);

                ushort peakMemory = 0;
                foreach(var snapshot in history)
                {
                    if(snapshot.MemoryUsageMB > peakMemory)
                    {
                        peakMemory = snapshot.MemoryUsageMB;
                    }
                }

                _dynamicLimits[portIndex] = (ushort)Math.Min(peakMemory + MemoryDropValue, ushort.MaxValue);
                Log.Info($"Port {port} warmup done. Base: {peakMemory}MB, Limit: {_dynamicLimits[portIndex]}MB", "MemoryAnalyzer");
                return;

            }

        }
        if (currentMb > _dynamicLimits[portIndex])
        {
            ref byte viol = ref _violations[portIndex];
            viol++;
            
            switch(_workMode)
            {
            
                case MemoryAnalyzerMods.Monitor:
                    {
                        if(viol > 10 && viol < 13)
                        {
                            Log.Info($"Instance on PORT:{port} use more memory: {_violations[portIndex]} times.", "MemoryAnalyzer");
                        }

                    }
                break;

                case MemoryAnalyzerMods.Balanced:
                    {

                        if(viol == 10)
                        {
                            // todo kill process
                            Log.Error($"Instance on PORT:{port} was killed.", "MemoryAnalyzer");
                            Clear(port);

                        }
                        else if(viol == 7)
                        {
                            Log.Warning($"Instance on PORT:{port} use more memory: {_violations[portIndex]} times.", "MemoryAnalyzer");
                        }
                        else if(viol == 3)
                        {
                            Log.Info($"Instance on PORT:{port} use more memory: {_violations[portIndex]} times.", "MemoryAnalyzer");
                        }

                    }
                break;

                case MemoryAnalyzerMods.Aggressive: throw new NotImplementedException();

            }

        }
        else
        {
            _violations[portIndex] = 0;
        }
        

    }


    public void Dispose()
    {
        
        if(_memoryPtr != null)
        {
            NativeMemory.Free(_memoryPtr);
        }

    }

    

}
