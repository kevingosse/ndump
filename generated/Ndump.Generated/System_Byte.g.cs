#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Byte : global::_.System.Object
{
    private Byte(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Byte(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }

    public byte m_value => Field<byte>();

    public static Byte FromAddress(ulong address, DumpContext ctx)
        => new Byte(address, ctx);

    public static Byte FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new Byte(address, ctx, arrayAddr, arrayIndex);

    public override string ToString() => $"Byte@0x{_objAddress:X}";
}
