#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class ListHolder : _.System.Object
{
    private ListHolder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Collections.Generic.List<string>? _items => Field<_.System.Collections.Generic.List<string>>();

    public _.System.Collections.Generic.List<_.Ndump.TestApp.Order>? _orders => Field<_.System.Collections.Generic.List<_.Ndump.TestApp.Order>>();

    public static new ListHolder FromAddress(ulong address, DumpContext ctx)
        => new ListHolder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ListHolder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.ListHolder"))
            yield return new ListHolder(addr, ctx);
    }

    public override string ToString() => $"ListHolder@0x{_objAddress:X}";
}
