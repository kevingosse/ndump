#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class EqualityComparer<T> : global::_.System.Object
{
    private EqualityComparer(ulong address, DumpContext context) : base(address, context) { }

    public static new EqualityComparer<T> FromAddress(ulong address, DumpContext context)
        => new EqualityComparer<T>(address, context);

    public override string ToString() => $"EqualityComparer@0x{_objAddress:X}";
}
