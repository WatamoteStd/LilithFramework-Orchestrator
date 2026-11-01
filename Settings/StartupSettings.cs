
namespace Orchestrator.Settings;

public record struct StartupSettings(ushort PortCount, ushort StartPort, bool MemoryLeakDrop, ushort MemoryDropValue, ushort PacketsForPush);