#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class AllPrimitives : _.System.Object
{
    private AllPrimitives(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _boolVal => Field<bool>();

    public byte _byteVal => Field<byte>();

    public sbyte _sbyteVal => Field<sbyte>();

    public short _shortVal => Field<short>();

    public ushort _ushortVal => Field<ushort>();

    public int _intVal => Field<int>();

    public uint _uintVal => Field<uint>();

    public long _longVal => Field<long>();

    public ulong _ulongVal => Field<ulong>();

    public float _floatVal => Field<float>();

    public double _doubleVal => Field<double>();

    public char _charVal => Field<char>();

    public static new AllPrimitives FromAddress(ulong address, DumpContext ctx)
        => new AllPrimitives(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<AllPrimitives> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.AllPrimitives"))
            yield return new AllPrimitives(addr, ctx);
    }

    public override string ToString() => $"AllPrimitives@0x{_objAddress:X}";
}
