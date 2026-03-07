#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
{
    private NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer FromAddress(ulong address, DumpContext ctx)
        => new NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalIgnoreCaseComparer"))
            yield return new NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer(addr, ctx);
    }

    public override string ToString() => $"NonRandomizedStringEqualityComparer_OrdinalIgnoreCaseComparer@0x{_objAddress:X}";
}
