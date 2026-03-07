#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class NonRandomizedStringEqualityComparer_OrdinalComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
{
    private NonRandomizedStringEqualityComparer_OrdinalComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new NonRandomizedStringEqualityComparer_OrdinalComparer FromAddress(ulong address, DumpContext ctx)
        => new NonRandomizedStringEqualityComparer_OrdinalComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<NonRandomizedStringEqualityComparer_OrdinalComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalComparer"))
            yield return new NonRandomizedStringEqualityComparer_OrdinalComparer(addr, ctx);
    }

    public override string ToString() => $"NonRandomizedStringEqualityComparer_OrdinalComparer@0x{_objAddress:X}";
}
