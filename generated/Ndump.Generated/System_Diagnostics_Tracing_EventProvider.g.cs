#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProvider : _.System.Object
{
    protected EventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Diagnostics.Tracing.EventProviderImpl? _eventProvider
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_eventProvider");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Diagnostics.Tracing.EventProviderImpl ?? _.System.Diagnostics.Tracing.EventProviderImpl.FromAddress(addr, _ctx);
        }
    }

    public string? _providerName => _ctx.GetStringField(_objAddress, "_providerName");

    // ValueType field: _providerId (object) — not yet supported

    public bool _disposed => _ctx.GetFieldValue<bool>(_objAddress, "_disposed");

    public static new EventProvider FromAddress(ulong address, DumpContext ctx)
        => new EventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventProvider"))
            yield return new EventProvider(addr, ctx);
    }

    public override string ToString() => $"EventProvider@0x{_objAddress:X}";
}
