#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProvider : _.System.Object
{
    protected EventProvider(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Diagnostics.Tracing.EventProviderImpl? _eventProvider => Field<_.System.Diagnostics.Tracing.EventProviderImpl>();

    public string? _providerName => Field<string>();

    public _.System.Guid _providerId => StructField<_.System.Guid>("System.Guid");

    public bool _disposed => Field<bool>();

    public static new EventProvider FromAddress(ulong address, DumpContext context)
        => new EventProvider(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EventProvider> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.EventProvider"))
            yield return new EventProvider(addr, context);
    }

    public override string ToString() => $"EventProvider@0x{_objAddress:X}";
}
