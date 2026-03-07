#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EventPipeEventProvider : _.System.Diagnostics.Tracing.EventProviderImpl
{
    private EventPipeEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>? _eventProvider => Field<_.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>>();

    public nint _provHandle => Field<nint>();

    public _.System.Runtime.InteropServices.GCHandle<_.System.Diagnostics.Tracing.EventPipeEventProvider> _gcHandle => StructField<_.System.Runtime.InteropServices.GCHandle<_.System.Diagnostics.Tracing.EventPipeEventProvider>>("System.Runtime.InteropServices.GCHandle<System.Diagnostics.Tracing.EventPipeEventProvider>");

    public static new EventPipeEventProvider FromAddress(ulong address, DumpContext ctx)
        => new EventPipeEventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventPipeEventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventPipeEventProvider"))
            yield return new EventPipeEventProvider(addr, ctx);
    }

    public override string ToString() => $"EventPipeEventProvider@0x{_objAddress:X}";
}
