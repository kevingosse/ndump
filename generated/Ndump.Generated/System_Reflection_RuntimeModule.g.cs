#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public sealed class RuntimeModule : _.System.Reflection.Module
{
    private RuntimeModule(ulong address, DumpContext context) : base(address, context) { }

    public _.System.RuntimeType? m_runtimeType => Field<_.System.RuntimeType>();

    public _.System.Reflection.RuntimeAssembly? m_runtimeAssembly => Field<_.System.Reflection.RuntimeAssembly>();

    public nint m_pData => Field<nint>();

    public static new RuntimeModule FromAddress(ulong address, DumpContext context)
        => new RuntimeModule(address, context);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeModule> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.RuntimeModule"))
            yield return new RuntimeModule(addr, context);
    }

    public override string ToString() => $"RuntimeModule@0x{_objAddress:X}";
}
