#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventSource : _.System.Object
{
    protected EventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? m_name => _ctx.GetStringField(_objAddress, "m_name");

    public int m_id => _ctx.GetFieldValue<int>(_objAddress, "m_id");

    // ValueType field: m_guid (object) — not yet supported

    public ulong m_eventData => _ctx.GetObjectAddress(_objAddress, "m_eventData");

    public global::Ndump.Core.DumpArray<byte>? m_rawManifest
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_rawManifest");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    // Unknown field: m_eventCommandExecuted (object)

    public int m_config => _ctx.GetFieldValue<int>(_objAddress, "m_config");

    public bool m_eventSourceDisposed => _ctx.GetFieldValue<bool>(_objAddress, "m_eventSourceDisposed");

    public bool m_eventSourceEnabled => _ctx.GetFieldValue<bool>(_objAddress, "m_eventSourceEnabled");

    public int m_level => _ctx.GetFieldValue<int>(_objAddress, "m_level");

    public long m_matchAnyKeyword => _ctx.GetFieldValue<long>(_objAddress, "m_matchAnyKeyword");

    public ulong m_Dispatchers => _ctx.GetObjectAddress(_objAddress, "m_Dispatchers");

    public _.System.Diagnostics.Tracing.EventSource_OverrideEventProvider? m_etwProvider
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_etwProvider");
            return addr == 0 ? null : _.System.Diagnostics.Tracing.EventSource_OverrideEventProvider.FromAddress(addr, _ctx);
        }
    }

    public _.System.Object? m_createEventLock
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_createEventLock");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public nint m_writeEventStringEventHandle => _ctx.GetFieldValue<nint>(_objAddress, "m_writeEventStringEventHandle");

    public _.System.Diagnostics.Tracing.EventSource_OverrideEventProvider? m_eventPipeProvider
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_eventPipeProvider");
            return addr == 0 ? null : _.System.Diagnostics.Tracing.EventSource_OverrideEventProvider.FromAddress(addr, _ctx);
        }
    }

    public bool m_completelyInited => _ctx.GetFieldValue<bool>(_objAddress, "m_completelyInited");

    public _.System.Exception? m_constructionException
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_constructionException");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Exception ?? _.System.Exception.FromAddress(addr, _ctx);
        }
    }

    public byte m_outOfBandMessageCount => _ctx.GetFieldValue<byte>(_objAddress, "m_outOfBandMessageCount");

    public ulong m_deferredCommands => _ctx.GetObjectAddress(_objAddress, "m_deferredCommands");

    // Array field: m_traits (object) — element type not supported

    public global::Ndump.Core.DumpArray<ulong>? m_channelData
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_channelData");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<ulong>(addr, len, i => _ctx.GetArrayElementValue<ulong>(addr, i));
        }
    }

    public _.System.Diagnostics.Tracing.ActivityTracker? m_activityTracker
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_activityTracker");
            return addr == 0 ? null : _.System.Diagnostics.Tracing.ActivityTracker.FromAddress(addr, _ctx);
        }
    }

    public global::Ndump.Core.DumpArray<byte>? m_providerMetadata
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_providerMetadata");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public _.System.Diagnostics.Tracing.TraceLoggingEventHandleTable? m_eventHandleTable
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_eventHandleTable");
            return addr == 0 ? null : _.System.Diagnostics.Tracing.TraceLoggingEventHandleTable.FromAddress(addr, _ctx);
        }
    }

    public static new EventSource FromAddress(ulong address, DumpContext ctx)
        => new EventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventSource"))
            yield return new EventSource(addr, ctx);
    }

    public override string ToString() => $"EventSource@0x{_objAddress:X}";
}
