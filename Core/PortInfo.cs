
namespace Orchestrator.Core;

public struct PortInfo
{
    
    public ushort Port {get;}
    private ulong _useCount = 0;
    public bool IsBusy {get; set;}

    public PortInfo(ushort port)
    {
        Port = port;
        IsBusy = false;
    }

    public void UsePort() => _useCount++;
    public ulong Info() => _useCount;

}