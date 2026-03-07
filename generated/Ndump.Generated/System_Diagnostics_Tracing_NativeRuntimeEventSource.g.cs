#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class NativeRuntimeEventSource : _.System.Diagnostics.Tracing.EventSource
{
    private NativeRuntimeEventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new NativeRuntimeEventSource FromAddress(ulong address, DumpContext ctx)
        => new NativeRuntimeEventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<NativeRuntimeEventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.NativeRuntimeEventSource"))
            yield return new NativeRuntimeEventSource(addr, ctx);
    }

    public override string ToString() => $"NativeRuntimeEventSource@0x{_objAddress:X}";
}
