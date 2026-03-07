#nullable enable
using Ndump.Core;

namespace _.System;

public class StringComparer : _.System.Object
{
    protected StringComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new StringComparer FromAddress(ulong address, DumpContext ctx)
        => new StringComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StringComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.StringComparer"))
            yield return new StringComparer(addr, ctx);
    }

    public override string ToString() => $"StringComparer@0x{_objAddress:X}";
}
