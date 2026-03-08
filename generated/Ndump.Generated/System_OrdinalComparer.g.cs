#nullable enable
using Ndump.Core;

namespace _.System;

public class OrdinalComparer : _.System.StringComparer
{
    protected OrdinalComparer(ulong address, DumpContext context) : base(address, context) { }

    public bool _ignoreCase => Field<bool>();

    public static new OrdinalComparer FromAddress(ulong address, DumpContext context)
        => new OrdinalComparer(address, context);

    public static new global::System.Collections.Generic.IEnumerable<OrdinalComparer> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.OrdinalComparer"))
            yield return new OrdinalComparer(addr, context);
    }

    public override string ToString() => $"OrdinalComparer@0x{_objAddress:X}";
}
