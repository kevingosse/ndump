#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Byte : global::_.System.Object, global::Ndump.Core.IProxy<Byte>
{
    private Byte(ulong address, DumpContext context) : base(address, context) { }
    private Byte(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public byte m_value => Field<byte>();

    public static new Byte FromAddress(ulong address, DumpContext context)
        => new Byte(address, context);

    public static Byte FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Byte(address, context, interiorTypeName);

    public override string ToString() => $"Byte@0x{_objAddress:X}";
}
