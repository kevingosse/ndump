#nullable enable
using Ndump.Core;

namespace _.System;

public class Delegate : _.System.Object
{
    protected Delegate(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Object? _target => Field<_.System.Object>();

    public _.System.Object? _methodBase => Field<_.System.Object>();

    public nint _methodPtr => Field<nint>();

    public nint _methodPtrAux => Field<nint>();

    public static new Delegate FromAddress(ulong address, DumpContext ctx)
        => new Delegate(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Delegate> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Delegate"))
            yield return new Delegate(addr, ctx);
    }

    public override string ToString() => $"Delegate@0x{_objAddress:X}";
}
