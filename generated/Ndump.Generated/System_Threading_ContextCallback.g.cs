#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class ContextCallback : _.System.MulticastDelegate
{
    private ContextCallback(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new ContextCallback FromAddress(ulong address, DumpContext ctx)
        => new ContextCallback(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ContextCallback> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.ContextCallback"))
            yield return new ContextCallback(addr, ctx);
    }

    public override string ToString() => $"ContextCallback@0x{_objAddress:X}";
}
