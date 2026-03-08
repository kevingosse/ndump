#nullable enable
using Ndump.Core;

namespace _.System;

public sealed partial class DateTime : global::_.System.Object, global::Ndump.Core.IProxy<DateTime>
{
    private DateTime(ulong address, DumpContext context) : base(address, context) { }
    private DateTime(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public ulong _dateData => Field<ulong>();

    public static new DateTime FromAddress(ulong address, DumpContext context)
        => new DateTime(address, context);

    public static DateTime FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new DateTime(address, context, interiorTypeName);

    public override string ToString() => $"DateTime@0x{_objAddress:X}";
}
