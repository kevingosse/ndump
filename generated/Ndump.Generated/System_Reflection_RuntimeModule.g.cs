#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public sealed class RuntimeModule : _.System.Reflection.Module
{
    private RuntimeModule(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.RuntimeType? m_runtimeType => Field<_.System.RuntimeType>();

    public _.System.Reflection.RuntimeAssembly? m_runtimeAssembly => Field<_.System.Reflection.RuntimeAssembly>();

    public nint m_pData => Field<nint>();

    public static new RuntimeModule FromAddress(ulong address, DumpContext ctx)
        => new RuntimeModule(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeModule> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.RuntimeModule"))
            yield return new RuntimeModule(addr, ctx);
    }

    public override string ToString() => $"RuntimeModule@0x{_objAddress:X}";
}
