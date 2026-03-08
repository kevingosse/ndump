#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class TraceLoggingEventHandleTable : _.System.Object
{
    private TraceLoggingEventHandleTable(ulong address, DumpContext context) : base(address, context) { }

    public global::Ndump.Core.DumpArray<nint>? m_innerTable => ArrayField<nint>();

    public static new TraceLoggingEventHandleTable FromAddress(ulong address, DumpContext context)
        => new TraceLoggingEventHandleTable(address, context);

    public static new global::System.Collections.Generic.IEnumerable<TraceLoggingEventHandleTable> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.TraceLoggingEventHandleTable"))
            yield return new TraceLoggingEventHandleTable(addr, context);
    }

    public override string ToString() => $"TraceLoggingEventHandleTable@0x{_objAddress:X}";
}
