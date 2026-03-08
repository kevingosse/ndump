#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProviderImpl : _.System.Object
{
    protected EventProviderImpl(ulong address, DumpContext context) : base(address, context) { }

    public byte _level => Field<byte>();

    public long _anyKeywordMask => Field<long>();

    public long _allKeywordMask => Field<long>();

    public bool _enabled => Field<bool>();

    public static new EventProviderImpl FromAddress(ulong address, DumpContext context)
        => new EventProviderImpl(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EventProviderImpl> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.EventProviderImpl"))
            yield return new EventProviderImpl(addr, context);
    }

    public override string ToString() => $"EventProviderImpl@0x{_objAddress:X}";
}
