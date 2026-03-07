#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public partial class EventSource : _.System.Object
{
    protected EventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? m_name => Field<string>();

    public int m_id => Field<int>();

    // ValueType field: m_guid (object) — not yet supported

    public ulong m_eventData => RefAddress();

    // Array field: m_rawManifest (object) — element type not supported

    // Unknown field: m_eventCommandExecuted (object)

    public int m_config => Field<int>();

    public bool m_eventSourceDisposed => Field<bool>();

    public bool m_eventSourceEnabled => Field<bool>();

    public int m_level => Field<int>();

    public long m_matchAnyKeyword => Field<long>();

    public ulong m_Dispatchers => RefAddress();

    public _.System.Diagnostics.Tracing.EventSource.OverrideEventProvider? m_etwProvider => Field<_.System.Diagnostics.Tracing.EventSource.OverrideEventProvider>();

    public _.System.Object? m_createEventLock => Field<_.System.Object>();

    public nint m_writeEventStringEventHandle => Field<nint>();

    public _.System.Diagnostics.Tracing.EventSource.OverrideEventProvider? m_eventPipeProvider => Field<_.System.Diagnostics.Tracing.EventSource.OverrideEventProvider>();

    public bool m_completelyInited => Field<bool>();

    public _.System.Exception? m_constructionException => Field<_.System.Exception>();

    public byte m_outOfBandMessageCount => Field<byte>();

    public ulong m_deferredCommands => RefAddress();

    // Array field: m_traits (object) — element type not supported

    public global::Ndump.Core.DumpArray<ulong>? m_channelData
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<ulong>(addr, len, i => _ctx.GetArrayElementValue<ulong>(addr, i));
        }
    }

    public _.System.Diagnostics.Tracing.ActivityTracker? m_activityTracker => Field<_.System.Diagnostics.Tracing.ActivityTracker>();

    // Array field: m_providerMetadata (object) — element type not supported

    public _.System.Diagnostics.Tracing.TraceLoggingEventHandleTable? m_eventHandleTable => Field<_.System.Diagnostics.Tracing.TraceLoggingEventHandleTable>();

    public static new EventSource FromAddress(ulong address, DumpContext ctx)
        => new EventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventSource"))
            yield return new EventSource(addr, ctx);
    }

    public override string ToString() => $"EventSource@0x{_objAddress:X}";
}
