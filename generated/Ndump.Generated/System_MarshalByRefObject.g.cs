#nullable enable
using Ndump.Core;

namespace _.System;

public class MarshalByRefObject : _.System.Object
{
    protected MarshalByRefObject(ulong address, DumpContext context) : base(address, context) { }

    public static new MarshalByRefObject FromAddress(ulong address, DumpContext context)
        => new MarshalByRefObject(address, context);

    public static new global::System.Collections.Generic.IEnumerable<MarshalByRefObject> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.MarshalByRefObject"))
            yield return new MarshalByRefObject(addr, context);
    }

    public override string ToString() => $"MarshalByRefObject@0x{_objAddress:X}";
}
