#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Guid : _.System.ValueType
{
    private Guid(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _a => _ctx.GetFieldValue<int>(_objAddress, "_a");

    public short _b => _ctx.GetFieldValue<short>(_objAddress, "_b");

    public short _c => _ctx.GetFieldValue<short>(_objAddress, "_c");

    public byte _d => _ctx.GetFieldValue<byte>(_objAddress, "_d");

    public byte _e => _ctx.GetFieldValue<byte>(_objAddress, "_e");

    public byte _f => _ctx.GetFieldValue<byte>(_objAddress, "_f");

    public byte _g => _ctx.GetFieldValue<byte>(_objAddress, "_g");

    public byte _h => _ctx.GetFieldValue<byte>(_objAddress, "_h");

    public byte _i => _ctx.GetFieldValue<byte>(_objAddress, "_i");

    public byte _j => _ctx.GetFieldValue<byte>(_objAddress, "_j");

    public byte _k => _ctx.GetFieldValue<byte>(_objAddress, "_k");

    public static new Guid FromAddress(ulong address, DumpContext ctx)
        => new Guid(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Guid> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Guid"))
            yield return new Guid(addr, ctx);
    }

    public override string ToString() => $"Guid@0x{_objAddress:X}";
}
