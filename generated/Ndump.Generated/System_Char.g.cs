#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Char : global::_.System.Object
{
    private Char(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Char(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }

    public char m_value => Field<char>();

    public static Char FromAddress(ulong address, DumpContext ctx)
        => new Char(address, ctx);

    public static Char FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new Char(address, ctx, arrayAddr, arrayIndex);

    public override string ToString() => $"Char@0x{_objAddress:X}";
}
