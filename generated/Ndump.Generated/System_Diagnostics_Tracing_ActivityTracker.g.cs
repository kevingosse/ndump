#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class ActivityTracker : _.System.Object
{
    private ActivityTracker(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Threading.AsyncLocal<object>? m_current => Field<_.System.Threading.AsyncLocal<object>>();

    public bool m_checkedForEnable => Field<bool>();

    public static new ActivityTracker FromAddress(ulong address, DumpContext ctx)
        => new ActivityTracker(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ActivityTracker> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.ActivityTracker"))
            yield return new ActivityTracker(addr, ctx);
    }

    public override string ToString() => $"ActivityTracker@0x{_objAddress:X}";
}
