#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public sealed class RuntimeAssembly : _.System.Reflection.Assembly
{
    private RuntimeAssembly(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _ModuleResolve => RefAddress();

    public string? m_fullname => Field<string>();

    public _.System.Object? m_syncRoot => Field<_.System.Object>();

    public nint m_assembly => Field<nint>();

    public static new RuntimeAssembly FromAddress(ulong address, DumpContext ctx)
        => new RuntimeAssembly(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeAssembly> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.RuntimeAssembly"))
            yield return new RuntimeAssembly(addr, ctx);
    }

    public override string ToString() => $"RuntimeAssembly@0x{_objAddress:X}";
}
