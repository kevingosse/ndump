#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class TypeInfo : _.System.Type
{
    protected TypeInfo(ulong address, DumpContext context) : base(address, context) { }

    public static new TypeInfo FromAddress(ulong address, DumpContext context)
        => new TypeInfo(address, context);

    public static new global::System.Collections.Generic.IEnumerable<TypeInfo> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.TypeInfo"))
            yield return new TypeInfo(addr, context);
    }

    public override string ToString() => $"TypeInfo@0x{_objAddress:X}";
}
