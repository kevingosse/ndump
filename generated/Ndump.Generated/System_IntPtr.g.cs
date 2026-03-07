#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class IntPtr : global::_.System.Object
{
    private IntPtr(ulong address, DumpContext ctx) : base(address, ctx) { }
    private IntPtr(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private IntPtr(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public nint _value => Field<nint>();

    public static new IntPtr FromAddress(ulong address, DumpContext ctx)
        => new IntPtr(address, ctx);

    public static IntPtr FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new IntPtr(address, ctx, arrayAddr, arrayIndex);

    public static IntPtr FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new IntPtr(address, ctx, interiorTypeName);

    public override string ToString() => $"IntPtr@0x{_objAddress:X}";
}
