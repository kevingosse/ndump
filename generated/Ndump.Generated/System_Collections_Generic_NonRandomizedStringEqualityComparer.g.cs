#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer : _.System.Object
{
    protected NonRandomizedStringEqualityComparer(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _underlyingComparer => Field<global::_.System.Object>();

    public static new NonRandomizedStringEqualityComparer FromAddress(ulong address, DumpContext context)
        => new NonRandomizedStringEqualityComparer(address, context);

    public static new global::System.Collections.Generic.IEnumerable<NonRandomizedStringEqualityComparer> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer"))
            yield return new NonRandomizedStringEqualityComparer(addr, context);
    }

    public override string ToString() => $"NonRandomizedStringEqualityComparer@0x{_objAddress:X}";
}
