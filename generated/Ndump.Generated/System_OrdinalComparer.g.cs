#nullable enable
using Ndump.Core;

namespace _.System;

public class OrdinalComparer : _.System.StringComparer
{
    protected OrdinalComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _ignoreCase => Field<bool>();

    public static new OrdinalComparer FromAddress(ulong address, DumpContext ctx)
        => new OrdinalComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OrdinalComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.OrdinalComparer"))
            yield return new OrdinalComparer(addr, ctx);
    }

    public override string ToString() => $"OrdinalComparer@0x{_objAddress:X}";
}
