#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class OrdinalCaseSensitiveComparer : _.System.OrdinalComparer
{
    private OrdinalCaseSensitiveComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new OrdinalCaseSensitiveComparer FromAddress(ulong address, DumpContext ctx)
        => new OrdinalCaseSensitiveComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OrdinalCaseSensitiveComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.OrdinalCaseSensitiveComparer"))
            yield return new OrdinalCaseSensitiveComparer(addr, ctx);
    }

    public override string ToString() => $"OrdinalCaseSensitiveComparer@0x{_objAddress:X}";
}
