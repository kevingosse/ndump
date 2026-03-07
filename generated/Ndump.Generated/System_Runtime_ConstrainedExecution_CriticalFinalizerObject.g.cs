#nullable enable
using Ndump.Core;

namespace _.System.Runtime.ConstrainedExecution;

public class CriticalFinalizerObject : _.System.Object
{
    protected CriticalFinalizerObject(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new CriticalFinalizerObject FromAddress(ulong address, DumpContext ctx)
        => new CriticalFinalizerObject(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<CriticalFinalizerObject> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Runtime.ConstrainedExecution.CriticalFinalizerObject"))
            yield return new CriticalFinalizerObject(addr, ctx);
    }

    public override string ToString() => $"CriticalFinalizerObject@0x{_objAddress:X}";
}
