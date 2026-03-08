#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class WeakReference<T> : global::_.System.Object
{
    private WeakReference(ulong address, DumpContext context) : base(address, context) { }

    public nint _taggedHandle => Field<nint>();

    public static new WeakReference<T> FromAddress(ulong address, DumpContext context)
        => new WeakReference<T>(address, context);

    public override string ToString() => $"WeakReference@0x{_objAddress:X}";
}
