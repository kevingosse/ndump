#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Middle : _.Ndump.TestApp.Base
{
    protected Middle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _middleField => Field<string>();

    public static new Middle FromAddress(ulong address, DumpContext ctx)
        => new Middle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Middle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Middle"))
            yield return new Middle(addr, ctx);
    }

    public override string ToString() => $"Middle@0x{_objAddress:X}";
}
