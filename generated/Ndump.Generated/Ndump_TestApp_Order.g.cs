#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Order : _.System.Object
{
    private Order(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _orderId => _ctx.GetFieldValue<int>(_objAddress, "_orderId");

    public double _total => _ctx.GetFieldValue<double>(_objAddress, "_total");

    public string? _description => _ctx.GetStringField(_objAddress, "_description");

    // ValueType field: _createdAt (object) — not yet supported

    public static new Order FromAddress(ulong address, DumpContext ctx)
        => new Order(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Order> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Order"))
            yield return new Order(addr, ctx);
    }

    public override string ToString() => $"Order@0x{_objAddress:X}";
}
