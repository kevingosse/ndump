#nullable enable
using Ndump.Core;

namespace _.System.Runtime.ConstrainedExecution;

public class CriticalFinalizerObject : _.System.Object
{
    protected CriticalFinalizerObject(ulong address, DumpContext context) : base(address, context) { }

    public static new CriticalFinalizerObject FromAddress(ulong address, DumpContext context)
        => new CriticalFinalizerObject(address, context);

    public static new global::System.Collections.Generic.IEnumerable<CriticalFinalizerObject> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Runtime.ConstrainedExecution.CriticalFinalizerObject"))
            yield return new CriticalFinalizerObject(addr, context);
    }

    public override string ToString() => $"CriticalFinalizerObject@0x{_objAddress:X}";
}
