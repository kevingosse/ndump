#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class WeakReference<T> : global::_.System.Object
{
    private WeakReference(ulong address, DumpContext ctx) : base(address, ctx) { }

    public nint _taggedHandle => Field<nint>();

    public static new WeakReference<T> FromAddress(ulong address, DumpContext ctx)
        => new WeakReference<T>(address, ctx);

    public override string ToString() => $"WeakReference@0x{_objAddress:X}";
}
