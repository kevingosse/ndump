#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EventSource_OverrideEventProvider : _.System.Diagnostics.Tracing.EventProvider
{
    private EventSource_OverrideEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Func_System_Diagnostics_Tracing_EventSource_? _eventSourceFactory
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_eventSourceFactory");
            return addr == 0 ? null : _.System.Func_System_Diagnostics_Tracing_EventSource_.FromAddress(addr, _ctx);
        }
    }

    public int _eventProviderType => _ctx.GetFieldValue<int>(_objAddress, "_eventProviderType");

    public static new EventSource_OverrideEventProvider FromAddress(ulong address, DumpContext ctx)
        => new EventSource_OverrideEventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventSource_OverrideEventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventSource+OverrideEventProvider"))
            yield return new EventSource_OverrideEventProvider(addr, ctx);
    }

    public override string ToString() => $"EventSource_OverrideEventProvider@0x{_objAddress:X}";
}
