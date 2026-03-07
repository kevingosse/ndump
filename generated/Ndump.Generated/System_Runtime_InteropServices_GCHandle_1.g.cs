#nullable enable
using Ndump.Core;

namespace _.System.Runtime.InteropServices;

public sealed class GCHandle<T> : global::_.System.Object
{
    private GCHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public nint _handle => Field<nint>();

    public static new GCHandle<T> FromAddress(ulong address, DumpContext ctx)
        => new GCHandle<T>(address, ctx);

    public override string ToString() => $"GCHandle@0x{_objAddress:X}";
}
