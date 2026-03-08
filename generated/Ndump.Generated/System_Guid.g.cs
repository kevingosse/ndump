#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Guid : global::_.System.Object, global::Ndump.Core.IProxy<Guid>
{
    private Guid(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Guid(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public int _a => Field<int>();

    public short _b => Field<short>();

    public short _c => Field<short>();

    public byte _d => Field<byte>();

    public byte _e => Field<byte>();

    public byte _f => Field<byte>();

    public byte _g => Field<byte>();

    public byte _h => Field<byte>();

    public byte _i => Field<byte>();

    public byte _j => Field<byte>();

    public byte _k => Field<byte>();

    public static new Guid FromAddress(ulong address, DumpContext ctx)
        => new Guid(address, ctx);

    public static Guid FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Guid(address, ctx, interiorTypeName);

    public override string ToString() => $"Guid@0x{_objAddress:X}";
}
