#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Double : global::_.System.Object, global::Ndump.Core.IProxy<Double>
{
    private Double(ulong address, DumpContext context) : base(address, context) { }
    private Double(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public double m_value => Field<double>();

    public static new Double FromAddress(ulong address, DumpContext context)
        => new Double(address, context);

    public static Double FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Double(address, context, interiorTypeName);

    public override string ToString() => $"Double@0x{_objAddress:X}";
}
