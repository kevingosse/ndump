#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class ContextCallback : _.System.MulticastDelegate
{
    private ContextCallback(ulong address, DumpContext context) : base(address, context) { }

    public static new ContextCallback FromAddress(ulong address, DumpContext context)
        => new ContextCallback(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ContextCallback> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Threading.ContextCallback"))
            yield return new ContextCallback(addr, context);
    }

    public override string ToString() => $"ContextCallback@0x{_objAddress:X}";
}
