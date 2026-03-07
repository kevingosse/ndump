#nullable enable
using Ndump.Core;

namespace _.System;

public class ValueType : _.System.Object
{
    protected ValueType(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new ValueType FromAddress(ulong address, DumpContext ctx)
        => new ValueType(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ValueType> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.ValueType"))
            yield return new ValueType(addr, ctx);
    }

    public override string ToString() => $"ValueType@0x{_objAddress:X}";
}
