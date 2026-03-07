#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EventPipeEventProvider : _.System.Diagnostics.Tracing.EventProviderImpl
{
    private EventPipeEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.WeakReference_System_Diagnostics_Tracing_EventProvider_? _eventProvider => Field<_.System.WeakReference_System_Diagnostics_Tracing_EventProvider_>();

    public nint _provHandle => Field<nint>();

    // ValueType field: _gcHandle (object) — not yet supported

    public static new EventPipeEventProvider FromAddress(ulong address, DumpContext ctx)
        => new EventPipeEventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventPipeEventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventPipeEventProvider"))
            yield return new EventPipeEventProvider(addr, ctx);
    }

    public override string ToString() => $"EventPipeEventProvider@0x{_objAddress:X}";
}
