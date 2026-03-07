#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer : _.System.Object
{
    protected NonRandomizedStringEqualityComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _underlyingComparer => RefAddress();

    public static new NonRandomizedStringEqualityComparer FromAddress(ulong address, DumpContext ctx)
        => new NonRandomizedStringEqualityComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<NonRandomizedStringEqualityComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer"))
            yield return new NonRandomizedStringEqualityComparer(addr, ctx);
    }

    public override string ToString() => $"NonRandomizedStringEqualityComparer@0x{_objAddress:X}";
}
