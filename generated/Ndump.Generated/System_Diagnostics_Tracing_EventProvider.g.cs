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
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Diagnostics.Tracing.EventProviderImpl ?? _.System.Diagnostics.Tracing.EventProviderImpl.FromAddress(addr, _ctx);
        }
    }

    public string? _providerName => StringField();

    // ValueType field: _providerId (object) — not yet supported

    public bool _disposed => Field<bool>();

    public static new EventProvider FromAddress(ulong address, DumpContext ctx)
        => new EventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventProvider"))
            yield return new EventProvider(addr, ctx);
    }

    public override string ToString() => $"EventProvider@0x{_objAddress:X}";
}
