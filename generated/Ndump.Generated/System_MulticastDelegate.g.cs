#nullable enable
using Ndump.Core;

namespace _.System;

public class MulticastDelegate : _.System.Delegate
{
    protected MulticastDelegate(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Object? _invocationList => Field<_.System.Object>();

    public nint _invocationCount => Field<nint>();

    public static new MulticastDelegate FromAddress(ulong address, DumpContext context)
        => new MulticastDelegate(address, context);

    public static new global::System.Collections.Generic.IEnumerable<MulticastDelegate> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.MulticastDelegate"))
            yield return new MulticastDelegate(addr, context);
    }

    public override string ToString() => $"MulticastDelegate@0x{_objAddress:X}";
}
