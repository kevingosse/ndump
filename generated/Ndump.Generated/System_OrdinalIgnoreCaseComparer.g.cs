#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class OrdinalIgnoreCaseComparer : _.System.OrdinalComparer
{
    private OrdinalIgnoreCaseComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new OrdinalIgnoreCaseComparer FromAddress(ulong address, DumpContext ctx)
        => new OrdinalIgnoreCaseComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OrdinalIgnoreCaseComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.OrdinalIgnoreCaseComparer"))
            yield return new OrdinalIgnoreCaseComparer(addr, ctx);
    }

    public override string ToString() => $"OrdinalIgnoreCaseComparer@0x{_objAddress:X}";
}
