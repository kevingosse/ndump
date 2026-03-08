#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Int32 : global::_.System.Object, global::Ndump.Core.IProxy<Int32>
{
    private Int32(ulong address, DumpContext context) : base(address, context) { }
    private Int32(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public int m_value => Field<int>();

    public static new Int32 FromAddress(ulong address, DumpContext context)
        => new Int32(address, context);

    public static Int32 FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Int32(address, context, interiorTypeName);

    public override string ToString() => $"Int32@0x{_objAddress:X}";
}
