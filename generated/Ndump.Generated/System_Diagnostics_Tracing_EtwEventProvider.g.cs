#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EtwEventProvider : _.System.Diagnostics.Tracing.EventProviderImpl
{
    private EtwEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>? _eventProvider => Field<_.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>>();

    public long _registrationHandle => Field<long>();

    // ValueType field: _gcHandle (object) — not yet supported

    public _.System.Collections.Generic.List<object>? _liveSessions => Field<_.System.Collections.Generic.List<object>>();

    // ValueType field: _providerId (object) — not yet supported

    public static new EtwEventProvider FromAddress(ulong address, DumpContext ctx)
        => new EtwEventProvider(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EtwEventProvider> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EtwEventProvider"))
            yield return new EtwEventProvider(addr, ctx);
    }

    public override string ToString() => $"EtwEventProvider@0x{_objAddress:X}";
}
