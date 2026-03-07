#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Nullable<T> : global::_.System.Object
{
    private Nullable(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool hasValue => Field<bool>();

    public global::_.System.Object? value => Field<global::_.System.Object>();

    public static new Nullable<T> FromAddress(ulong address, DumpContext ctx)
        => new Nullable<T>(address, ctx);

    public override string ToString() => $"Nullable@0x{_objAddress:X}";
}
