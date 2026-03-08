#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Base : _.System.Object
{
    protected Base(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _baseField => Field<int>();

    public static new Base FromAddress(ulong address, DumpContext ctx)
        => new Base(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Base> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Base"))
            yield return new Base(addr, ctx);
    }

    public override string ToString() => $"Base@0x{_objAddress:X}";
}
