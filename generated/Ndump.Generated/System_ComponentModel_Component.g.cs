#nullable enable
using Ndump.Core;

namespace _.System.ComponentModel;

public class Component : _.System.MarshalByRefObject
{
    protected Component(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _site => Field<global::_.System.Object>();

    public global::_.System.Object? _events => Field<global::_.System.Object>();

    public static new Component FromAddress(ulong address, DumpContext context)
        => new Component(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Component> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.ComponentModel.Component"))
            yield return new Component(addr, context);
    }

    public override string ToString() => $"Component@0x{_objAddress:X}";
}
