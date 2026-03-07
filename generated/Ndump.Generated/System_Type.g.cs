#nullable enable
using Ndump.Core;

namespace _.System;

public class Type : _.System.Reflection.MemberInfo
{
    protected Type(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Type FromAddress(ulong address, DumpContext ctx)
        => new Type(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Type> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Type"))
            yield return new Type(addr, ctx);
    }

    public override string ToString() => $"Type@0x{_objAddress:X}";
}
