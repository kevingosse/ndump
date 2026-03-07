#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Func<T> : global::_.System.Object
{
    private Func(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Object? _target => Field<_.System.Object>();

    public _.System.Object? _methodBase => Field<_.System.Object>();

    public nint _methodPtr => Field<nint>();

    public nint _methodPtrAux => Field<nint>();

    public _.System.Object? _invocationList => Field<_.System.Object>();

    public nint _invocationCount => Field<nint>();

    public static new Func<T> FromAddress(ulong address, DumpContext ctx)
        => new Func<T>(address, ctx);

    public override string ToString() => $"Func@0x{_objAddress:X}";
}
