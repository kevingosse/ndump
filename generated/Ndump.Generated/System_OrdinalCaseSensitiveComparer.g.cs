#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class OrdinalCaseSensitiveComparer : _.System.OrdinalComparer
{
    private OrdinalCaseSensitiveComparer(ulong address, DumpContext context) : base(address, context) { }

    public static new OrdinalCaseSensitiveComparer FromAddress(ulong address, DumpContext context)
        => new OrdinalCaseSensitiveComparer(address, context);

    public static new global::System.Collections.Generic.IEnumerable<OrdinalCaseSensitiveComparer> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.OrdinalCaseSensitiveComparer"))
            yield return new OrdinalCaseSensitiveComparer(addr, context);
    }

    public override string ToString() => $"OrdinalCaseSensitiveComparer@0x{_objAddress:X}";
}
