#nullable enable
using Ndump.Core;

namespace _.System;

public sealed partial class RuntimeType : _.System.Reflection.TypeInfo
{
    private RuntimeType(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Object? m_keepalive => Field<_.System.Object>();

    public nint m_cache => Field<nint>();

    public nint m_handle => Field<nint>();

    public static new RuntimeType FromAddress(ulong address, DumpContext context)
        => new RuntimeType(address, context);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeType> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.RuntimeType"))
            yield return new RuntimeType(addr, context);
    }

    public override string ToString() => $"RuntimeType@0x{_objAddress:X}";
}
