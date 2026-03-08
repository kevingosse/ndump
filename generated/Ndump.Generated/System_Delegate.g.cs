#nullable enable
using Ndump.Core;

namespace _.System;

public class Delegate : _.System.Object
{
    protected Delegate(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Object? _target => Field<_.System.Object>();

    public _.System.Object? _methodBase => Field<_.System.Object>();

    public nint _methodPtr => Field<nint>();

    public nint _methodPtrAux => Field<nint>();

    public static new Delegate FromAddress(ulong address, DumpContext context)
        => new Delegate(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Delegate> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Delegate"))
            yield return new Delegate(addr, context);
    }

    public override string ToString() => $"Delegate@0x{_objAddress:X}";
}
