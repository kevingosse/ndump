#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Array : _.System.Object
{
    private Array(ulong address, DumpContext context) : base(address, context) { }

    public static new Array FromAddress(ulong address, DumpContext context)
        => new Array(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Array> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Array"))
            yield return new Array(addr, context);
    }

    public override string ToString() => $"Array@0x{_objAddress:X}";
}
