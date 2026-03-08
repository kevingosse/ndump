#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Int32 : global::_.System.Object, global::Ndump.Core.IProxy<Int32>
{
    private Int32(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Int32(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public int m_value => Field<int>();

    public static new Int32 FromAddress(ulong address, DumpContext ctx)
        => new Int32(address, ctx);

    public static Int32 FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Int32(address, ctx, interiorTypeName);

    public override string ToString() => $"Int32@0x{_objAddress:X}";
}
