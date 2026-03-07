#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class TypeInfo : _.System.Type
{
    protected TypeInfo(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new TypeInfo FromAddress(ulong address, DumpContext ctx)
        => new TypeInfo(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<TypeInfo> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.TypeInfo"))
            yield return new TypeInfo(addr, ctx);
    }

    public override string ToString() => $"TypeInfo@0x{_objAddress:X}";
}
