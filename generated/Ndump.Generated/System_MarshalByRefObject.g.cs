#nullable enable
using Ndump.Core;

namespace _.System;

public class MarshalByRefObject : _.System.Object
{
    protected MarshalByRefObject(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new MarshalByRefObject FromAddress(ulong address, DumpContext ctx)
        => new MarshalByRefObject(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<MarshalByRefObject> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.MarshalByRefObject"))
            yield return new MarshalByRefObject(addr, ctx);
    }

    public override string ToString() => $"MarshalByRefObject@0x{_objAddress:X}";
}
