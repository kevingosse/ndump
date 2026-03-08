#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Boolean : global::_.System.Object, global::Ndump.Core.IProxy<Boolean>
{
    private Boolean(ulong address, DumpContext context) : base(address, context) { }
    private Boolean(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public bool m_value => Field<bool>();

    public static new Boolean FromAddress(ulong address, DumpContext context)
        => new Boolean(address, context);

    public static Boolean FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Boolean(address, context, interiorTypeName);

    public override string ToString() => $"Boolean@0x{_objAddress:X}";
}
