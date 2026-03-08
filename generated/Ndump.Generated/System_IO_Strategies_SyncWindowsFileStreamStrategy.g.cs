#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public sealed class SyncWindowsFileStreamStrategy : _.System.IO.Strategies.OSFileStreamStrategy
{
    private SyncWindowsFileStreamStrategy(ulong address, DumpContext context) : base(address, context) { }

    public static new SyncWindowsFileStreamStrategy FromAddress(ulong address, DumpContext context)
        => new SyncWindowsFileStreamStrategy(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SyncWindowsFileStreamStrategy> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.Strategies.SyncWindowsFileStreamStrategy"))
            yield return new SyncWindowsFileStreamStrategy(addr, context);
    }

    public override string ToString() => $"SyncWindowsFileStreamStrategy@0x{_objAddress:X}";
}
