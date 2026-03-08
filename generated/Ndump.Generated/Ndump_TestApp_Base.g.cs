#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Base : _.System.Object
{
    protected Base(ulong address, DumpContext context) : base(address, context) { }

    public int _baseField => Field<int>();

    public static new Base FromAddress(ulong address, DumpContext context)
        => new Base(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Base> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Base"))
            yield return new Base(addr, context);
    }

    public override string ToString() => $"Base@0x{_objAddress:X}";
}
