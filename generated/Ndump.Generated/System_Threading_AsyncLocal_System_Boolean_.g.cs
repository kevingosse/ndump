#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class AsyncLocal_System_Boolean_ : _.System.Object
{
    private AsyncLocal_System_Boolean_(ulong address, DumpContext ctx) : base(address, ctx) { }

    // Unknown field: _valueChangedHandler (object)

    public static new AsyncLocal_System_Boolean_ FromAddress(ulong address, DumpContext ctx)
        => new AsyncLocal_System_Boolean_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<AsyncLocal_System_Boolean_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.AsyncLocal<System.Boolean>"))
            yield return new AsyncLocal_System_Boolean_(addr, ctx);
    }

    public override string ToString() => $"AsyncLocal_System_Boolean_@0x{_objAddress:X}";
}
