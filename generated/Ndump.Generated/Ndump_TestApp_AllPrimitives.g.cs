#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class AllPrimitives : _.System.Object
{
    private AllPrimitives(ulong address, DumpContext context) : base(address, context) { }

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

    public static new AllPrimitives FromAddress(ulong address, DumpContext context)
        => new AllPrimitives(address, context);

    public static new global::System.Collections.Generic.IEnumerable<AllPrimitives> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.AllPrimitives"))
            yield return new AllPrimitives(addr, context);
    }

    public override string ToString() => $"AllPrimitives@0x{_objAddress:X}";
}
