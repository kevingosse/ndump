#nullable enable
using Ndump.Core;

namespace _.System.IO.Strategies;

public sealed class SyncWindowsFileStreamStrategy : _.System.IO.Strategies.OSFileStreamStrategy
{
    private SyncWindowsFileStreamStrategy(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new SyncWindowsFileStreamStrategy FromAddress(ulong address, DumpContext ctx)
        => new SyncWindowsFileStreamStrategy(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SyncWindowsFileStreamStrategy> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.Strategies.SyncWindowsFileStreamStrategy"))
            yield return new SyncWindowsFileStreamStrategy(addr, ctx);
    }

    public override string ToString() => $"SyncWindowsFileStreamStrategy@0x{_objAddress:X}";
}
