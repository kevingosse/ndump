#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Byte : global::_.System.Object
{
    private Byte(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Byte(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private Byte(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public byte m_value => Field<byte>();

    public static new Byte FromAddress(ulong address, DumpContext ctx)
        => new Byte(address, ctx);

    public static Byte FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new Byte(address, ctx, arrayAddr, arrayIndex);

    public static Byte FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Byte(address, ctx, interiorTypeName);

    public override string ToString() => $"Byte@0x{_objAddress:X}";
}
