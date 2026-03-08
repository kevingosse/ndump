#nullable enable
using Ndump.Core;

namespace _.System;

public class Type : _.System.Reflection.MemberInfo
{
    protected Type(ulong address, DumpContext context) : base(address, context) { }

    public static new Type FromAddress(ulong address, DumpContext context)
        => new Type(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Type> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Type"))
            yield return new Type(addr, context);
    }

    public override string ToString() => $"Type@0x{_objAddress:X}";
}
