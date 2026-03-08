#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Leaf : _.Ndump.TestApp.Middle
{
    private Leaf(ulong address, DumpContext ctx) : base(address, ctx) { }

    public double _leafField => Field<double>();

    public static new Leaf FromAddress(ulong address, DumpContext ctx)
        => new Leaf(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Leaf> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Leaf"))
            yield return new Leaf(addr, ctx);
    }

    public override string ToString() => $"Leaf@0x{_objAddress:X}";
}
