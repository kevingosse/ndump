#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class NativeRuntimeEventSource : _.System.Diagnostics.Tracing.EventSource
{
    private NativeRuntimeEventSource(ulong address, DumpContext context) : base(address, context) { }

    public static new NativeRuntimeEventSource FromAddress(ulong address, DumpContext context)
        => new NativeRuntimeEventSource(address, context);

    public static new global::System.Collections.Generic.IEnumerable<NativeRuntimeEventSource> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.NativeRuntimeEventSource"))
            yield return new NativeRuntimeEventSource(addr, context);
    }

    public override string ToString() => $"NativeRuntimeEventSource@0x{_objAddress:X}";
}
