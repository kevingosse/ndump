#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Guid : _.System.ValueType
{
    private Guid(ulong address, DumpContext ctx) : base(address, ctx) { }

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

    public static new global::System.Collections.Generic.IEnumerable<Guid> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Guid"))
            yield return new Guid(addr, ctx);
    }

    public override string ToString() => $"Guid@0x{_objAddress:X}";
}
