#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Char : global::_.System.Object, global::Ndump.Core.IProxy<Char>
{
    private Char(ulong address, DumpContext context) : base(address, context) { }
    private Char(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public char m_value => Field<char>();

    public static new Char FromAddress(ulong address, DumpContext context)
        => new Char(address, context);

    public static Char FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Char(address, context, interiorTypeName);

    public override string ToString() => $"Char@0x{_objAddress:X}";
}
