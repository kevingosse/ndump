#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class MemberInfo : _.System.Object
{
    protected MemberInfo(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new MemberInfo FromAddress(ulong address, DumpContext ctx)
        => new MemberInfo(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<MemberInfo> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.MemberInfo"))
            yield return new MemberInfo(addr, ctx);
    }

    public override string ToString() => $"MemberInfo@0x{_objAddress:X}";
}
