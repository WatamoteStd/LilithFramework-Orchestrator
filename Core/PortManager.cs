
using System.Collections.Concurrent;
using Orchestrator.Utils;

namespace Core;

public class PortManager
{
    
    private PortInfo[] _ports;

    public PortManager(int count, int firstIndex)
    {
        _ports = new PortInfo[count];
        
        for (int i = 0; i < count; i++)
        {
            
            _ports[i] = new PortInfo((ushort)(firstIndex + i));

        }

    }

    public bool TryGetPort(out ushort port)
    {
        port = 0;
        
        for(int i = 0; i < _ports.Length; i++)
        {
            
            if (!_ports[i].IsBusy)
            {
                port = _ports[i].Port;
                _ports[i].IsBusy = true;
                _ports[i].UsePort();
                return true;
            }

        }
        return false;

    }

    public void ReleasePort(ushort port)
    {
        
        for(int i = 0; i < _ports.Length; i++)
        {
            if (_ports[i].Port == port)
            {
                _ports[i].IsBusy = false;
                return;
            }
        }

    }

    public void ShowPortInfo(bool onlyUsed = true)
    {

        if(onlyUsed)
        {
            Log.Info("Active Ports Info", "Port Manager");
            for (int i = 0; i < _ports.Length; i++)
            {
                if(_ports[i].Info() == 0) continue;
                Console.WriteLine($"> Port:{_ports[i].Port} | TimesUsed:{_ports[i].Info()} <");
            }
        }
        else
        {
            Log.Info("Ports Info", "Port Manager");
            for (int i = 0; i < _ports.Length; i++)
            {
                Console.WriteLine($"> Port:{_ports[i].Port} | IsBusy:{_ports[i].IsBusy} <");
            }
        }
    }

}