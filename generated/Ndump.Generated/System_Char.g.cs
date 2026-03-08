#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Char : global::_.System.Object
{
    private Char(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Char(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public char m_value => Field<char>();

    public static new Char FromAddress(ulong address, DumpContext ctx)
        => new Char(address, ctx);

    public static Char FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Char(address, ctx, interiorTypeName);

    public override string ToString() => $"Char@0x{_objAddress:X}";
}
