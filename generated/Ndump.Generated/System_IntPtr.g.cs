#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class IntPtr : global::_.System.Object, global::Ndump.Core.IProxy<IntPtr>
{
    private IntPtr(ulong address, DumpContext context) : base(address, context) { }
    private IntPtr(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public nint _value => Field<nint>();

    public static new IntPtr FromAddress(ulong address, DumpContext context)
        => new IntPtr(address, context);

    public static IntPtr FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new IntPtr(address, context, interiorTypeName);

    public override string ToString() => $"IntPtr@0x{_objAddress:X}";
}
