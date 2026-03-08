#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class MemberInfo : _.System.Object
{
    protected MemberInfo(ulong address, DumpContext context) : base(address, context) { }

    public static new MemberInfo FromAddress(ulong address, DumpContext context)
        => new MemberInfo(address, context);

    public static new global::System.Collections.Generic.IEnumerable<MemberInfo> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.MemberInfo"))
            yield return new MemberInfo(addr, context);
    }

    public override string ToString() => $"MemberInfo@0x{_objAddress:X}";
}
