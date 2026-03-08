#nullable enable
using Ndump.Core;

namespace _.System;

public class StringComparer : _.System.Object
{
    protected StringComparer(ulong address, DumpContext context) : base(address, context) { }

    public static new StringComparer FromAddress(ulong address, DumpContext context)
        => new StringComparer(address, context);

    public static new global::System.Collections.Generic.IEnumerable<StringComparer> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.StringComparer"))
            yield return new StringComparer(addr, context);
    }

    public override string ToString() => $"StringComparer@0x{_objAddress:X}";
}
