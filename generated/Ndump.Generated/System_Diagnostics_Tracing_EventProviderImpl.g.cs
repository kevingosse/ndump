#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProviderImpl : _.System.Object
{
    protected EventProviderImpl(ulong address, DumpContext ctx) : base(address, ctx) { }

    public byte _level => Field<byte>();

    public long _anyKeywordMask => Field<long>();

    public long _allKeywordMask => Field<long>();

    public bool _enabled => Field<bool>();

    public static new EventProviderImpl FromAddress(ulong address, DumpContext ctx)
        => new EventProviderImpl(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventProviderImpl> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventProviderImpl"))
            yield return new EventProviderImpl(addr, ctx);
    }

    public override string ToString() => $"EventProviderImpl@0x{_objAddress:X}";
}
