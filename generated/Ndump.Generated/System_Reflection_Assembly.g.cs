#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class Assembly : _.System.Object
{
    protected Assembly(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Assembly FromAddress(ulong address, DumpContext ctx)
        => new Assembly(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Assembly> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.Assembly"))
            yield return new Assembly(addr, ctx);
    }

    public override string ToString() => $"Assembly@0x{_objAddress:X}";
}
