#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Double : global::_.System.Object
{
    private Double(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Double(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private Double(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public double m_value => Field<double>();

    public static new Double FromAddress(ulong address, DumpContext ctx)
        => new Double(address, ctx);

    public static Double FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new Double(address, ctx, arrayAddr, arrayIndex);

    public static Double FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Double(address, ctx, interiorTypeName);

    public override string ToString() => $"Double@0x{_objAddress:X}";
}
