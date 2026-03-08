#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class RuntimeFieldHandleInternal : global::_.System.Object
{
    private RuntimeFieldHandleInternal(ulong address, DumpContext ctx) : base(address, ctx) { }
    private RuntimeFieldHandleInternal(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private RuntimeFieldHandleInternal(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public nint m_handle => Field<nint>();

    public static new RuntimeFieldHandleInternal FromAddress(ulong address, DumpContext ctx)
        => new RuntimeFieldHandleInternal(address, ctx);

    public static RuntimeFieldHandleInternal FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new RuntimeFieldHandleInternal(address, ctx, arrayAddr, arrayIndex);

    public static RuntimeFieldHandleInternal FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new RuntimeFieldHandleInternal(address, ctx, interiorTypeName);

    public override string ToString() => $"RuntimeFieldHandleInternal@0x{_objAddress:X}";
}
