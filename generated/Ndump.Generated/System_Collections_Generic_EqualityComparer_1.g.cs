#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class EqualityComparer<T> : global::_.System.Object
{
    private EqualityComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new EqualityComparer<T> FromAddress(ulong address, DumpContext ctx)
        => new EqualityComparer<T>(address, ctx);

    public override string ToString() => $"EqualityComparer@0x{_objAddress:X}";
}
