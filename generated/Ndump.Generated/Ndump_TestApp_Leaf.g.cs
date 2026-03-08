#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Leaf : _.Ndump.TestApp.Middle
{
    private Leaf(ulong address, DumpContext context) : base(address, context) { }

    public double _leafField => Field<double>();

    public static new Leaf FromAddress(ulong address, DumpContext context)
        => new Leaf(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Leaf> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Leaf"))
            yield return new Leaf(addr, context);
    }

    public override string ToString() => $"Leaf@0x{_objAddress:X}";
}
