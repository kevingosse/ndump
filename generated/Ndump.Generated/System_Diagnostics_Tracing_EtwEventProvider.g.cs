#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class EtwEventProvider : _.System.Diagnostics.Tracing.EventProviderImpl
{
    private EtwEventProvider(ulong address, DumpContext context) : base(address, context) { }

    public _.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>? _eventProvider => Field<_.System.WeakReference<_.System.Diagnostics.Tracing.EventProvider>>();

    public long _registrationHandle => Field<long>();

    public _.System.Runtime.InteropServices.GCHandle<_.System.Diagnostics.Tracing.EtwEventProvider> _gcHandle => StructField<_.System.Runtime.InteropServices.GCHandle<_.System.Diagnostics.Tracing.EtwEventProvider>>("System.Runtime.InteropServices.GCHandle<System.Diagnostics.Tracing.EtwEventProvider>");

    public _.System.Collections.Generic.List<object>? _liveSessions => Field<_.System.Collections.Generic.List<object>>();

    public _.System.Guid _providerId => StructField<_.System.Guid>("System.Guid");

    public static new EtwEventProvider FromAddress(ulong address, DumpContext context)
        => new EtwEventProvider(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EtwEventProvider> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.EtwEventProvider"))
            yield return new EtwEventProvider(addr, context);
    }

    public override string ToString() => $"EtwEventProvider@0x{_objAddress:X}";
}
