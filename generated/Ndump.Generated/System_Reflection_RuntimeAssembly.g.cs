#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public sealed class RuntimeAssembly : _.System.Reflection.Assembly
{
    private RuntimeAssembly(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _ModuleResolve => Field<global::_.System.Object>();

    public string? m_fullname => Field<string>();

    public _.System.Object? m_syncRoot => Field<_.System.Object>();

    public nint m_assembly => Field<nint>();

    public static new RuntimeAssembly FromAddress(ulong address, DumpContext context)
        => new RuntimeAssembly(address, context);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeAssembly> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.RuntimeAssembly"))
            yield return new RuntimeAssembly(addr, context);
    }

    public override string ToString() => $"RuntimeAssembly@0x{_objAddress:X}";
}
