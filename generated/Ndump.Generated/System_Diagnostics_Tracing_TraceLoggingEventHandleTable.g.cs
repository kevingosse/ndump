#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class TraceLoggingEventHandleTable : _.System.Object
{
    private TraceLoggingEventHandleTable(ulong address, DumpContext ctx) : base(address, ctx) { }

    // Array field: m_innerTable (object) — element type not supported

    public static new TraceLoggingEventHandleTable FromAddress(ulong address, DumpContext ctx)
        => new TraceLoggingEventHandleTable(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<TraceLoggingEventHandleTable> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.TraceLoggingEventHandleTable"))
            yield return new TraceLoggingEventHandleTable(addr, ctx);
    }

    public override string ToString() => $"TraceLoggingEventHandleTable@0x{_objAddress:X}";
}
