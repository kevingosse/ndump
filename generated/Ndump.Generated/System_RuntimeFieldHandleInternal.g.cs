#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class RuntimeFieldHandleInternal : global::_.System.Object, global::Ndump.Core.IProxy<RuntimeFieldHandleInternal>
{
    private RuntimeFieldHandleInternal(ulong address, DumpContext context) : base(address, context) { }
    private RuntimeFieldHandleInternal(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public nint m_handle => Field<nint>();

    public static new RuntimeFieldHandleInternal FromAddress(ulong address, DumpContext context)
        => new RuntimeFieldHandleInternal(address, context);

    public static RuntimeFieldHandleInternal FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new RuntimeFieldHandleInternal(address, context, interiorTypeName);

    public override string ToString() => $"RuntimeFieldHandleInternal@0x{_objAddress:X}";
}
