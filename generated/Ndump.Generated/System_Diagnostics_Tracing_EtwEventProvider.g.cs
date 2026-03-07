#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EtwEventProvider : _.System.Diagnostics.Tracing.EventProviderImpl
{
    private EtwEventProvider(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.WeakReference_System_Diagnostics_Tracing_EventProvider_? _eventProvider
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_eventProvider");
            return addr == 0 ? null : _.System.WeakReference_System_Diagnostics_Tracing_EventProvider_.FromAddress(addr, _ctx);
        }
    }

    public long _registrationHandle => _ctx.GetFieldValue<long>(_objAddress, "_registrationHandle");

    // ValueType field: _gcHandle (object) — not yet supported

    public ulong _liveSessions => _ctx.GetObjectAddress(_objAddress, "_liveSessions");

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
