#nullable enable
using Ndump.Core;

namespace _.System;

public sealed partial class DateTime : global::_.System.Object
{
    private DateTime(ulong address, DumpContext ctx) : base(address, ctx) { }
    private DateTime(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private DateTime(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public ulong _dateData => Field<ulong>();

    public static new DateTime FromAddress(ulong address, DumpContext ctx)
        => new DateTime(address, ctx);

    public static DateTime FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new DateTime(address, ctx, arrayAddr, arrayIndex);

    public static DateTime FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new DateTime(address, ctx, interiorTypeName);

    public override string ToString() => $"DateTime@0x{_objAddress:X}";
}
