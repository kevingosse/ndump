#nullable enable
using Ndump.Core;

namespace _.System.Runtime.InteropServices;

public sealed class GCHandle<T> : global::_.System.Object, global::Ndump.Core.IProxy<GCHandle<T>>
{
    private GCHandle(ulong address, DumpContext context) : base(address, context) { }
    private GCHandle(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public nint _handle => Field<nint>();

    public static new GCHandle<T> FromAddress(ulong address, DumpContext context)
        => new GCHandle<T>(address, context);

    public static GCHandle<T> FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new GCHandle<T>(address, context, interiorTypeName);

    public override string ToString() => $"GCHandle@0x{_objAddress:X}";
}
