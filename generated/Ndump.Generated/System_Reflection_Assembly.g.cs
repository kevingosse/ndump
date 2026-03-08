#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class Assembly : _.System.Object
{
    protected Assembly(ulong address, DumpContext context) : base(address, context) { }

    public static new Assembly FromAddress(ulong address, DumpContext context)
        => new Assembly(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Assembly> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.Assembly"))
            yield return new Assembly(addr, context);
    }

    public override string ToString() => $"Assembly@0x{_objAddress:X}";
}
