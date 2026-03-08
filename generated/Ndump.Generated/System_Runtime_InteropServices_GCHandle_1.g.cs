#nullable enable
using Ndump.Core;

namespace _.System.Runtime.InteropServices;

public sealed class GCHandle<T> : global::_.System.Object, global::Ndump.Core.IProxy<GCHandle<T>>
{
    private GCHandle(ulong address, DumpContext ctx) : base(address, ctx) { }
    private GCHandle(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public nint _handle => Field<nint>();

    public static new GCHandle<T> FromAddress(ulong address, DumpContext ctx)
        => new GCHandle<T>(address, ctx);

    public static GCHandle<T> FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new GCHandle<T>(address, ctx, interiorTypeName);

    public override string ToString() => $"GCHandle@0x{_objAddress:X}";
}
