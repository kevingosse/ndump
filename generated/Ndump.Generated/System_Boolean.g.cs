#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Boolean : global::_.System.Object, global::Ndump.Core.IProxy<Boolean>
{
    private Boolean(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Boolean(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public bool m_value => Field<bool>();

    public static new Boolean FromAddress(ulong address, DumpContext ctx)
        => new Boolean(address, ctx);

    public static Boolean FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Boolean(address, ctx, interiorTypeName);

    public override string ToString() => $"Boolean@0x{_objAddress:X}";
}
