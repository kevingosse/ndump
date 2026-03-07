#nullable enable
using Ndump.Core;

namespace _.System;

public sealed partial class RuntimeType : _.System.Reflection.TypeInfo
{
    private RuntimeType(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Object? m_keepalive => Field<_.System.Object>();

    public nint m_cache => Field<nint>();

    public nint m_handle => Field<nint>();

    public static new RuntimeType FromAddress(ulong address, DumpContext ctx)
        => new RuntimeType(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeType> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.RuntimeType"))
            yield return new RuntimeType(addr, ctx);
    }

    public override string ToString() => $"RuntimeType@0x{_objAddress:X}";
}
