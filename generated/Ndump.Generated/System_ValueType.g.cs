#nullable enable
using Ndump.Core;

namespace _.System;

public class ValueType : _.System.Object
{
    protected ValueType(ulong address, DumpContext context) : base(address, context) { }

    public static new ValueType FromAddress(ulong address, DumpContext context)
        => new ValueType(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ValueType> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.ValueType"))
            yield return new ValueType(addr, context);
    }

    public override string ToString() => $"ValueType@0x{_objAddress:X}";
}
