#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Array : _.System.Object
{
    private Array(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Array FromAddress(ulong address, DumpContext ctx)
        => new Array(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Array> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Array"))
            yield return new Array(addr, ctx);
    }

    public override string ToString() => $"Array@0x{_objAddress:X}";
}
