#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProvider : _.System.Object
{
    protected EventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Diagnostics.Tracing.EventProviderImpl? _eventProvider => Field<_.System.Diagnostics.Tracing.EventProviderImpl>();

    public string? _providerName => Field<string>();

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
