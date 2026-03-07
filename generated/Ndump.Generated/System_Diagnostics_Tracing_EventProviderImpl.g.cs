#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public class EventProviderImpl : _.System.Object
{
    protected EventProviderImpl(ulong address, DumpContext ctx) : base(address, ctx) { }

    public byte _level => _ctx.GetFieldValue<byte>(_objAddress, "_level");

    public long _anyKeywordMask => _ctx.GetFieldValue<long>(_objAddress, "_anyKeywordMask");

    public long _allKeywordMask => _ctx.GetFieldValue<long>(_objAddress, "_allKeywordMask");

    public bool _enabled => _ctx.GetFieldValue<bool>(_objAddress, "_enabled");

    public static new EventProviderImpl FromAddress(ulong address, DumpContext ctx)
        => new EventProviderImpl(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EventProviderImpl> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.EventProviderImpl"))
            yield return new EventProviderImpl(addr, ctx);
    }

    public override string ToString() => $"EventProviderImpl@0x{_objAddress:X}";
}
