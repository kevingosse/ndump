#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class ActivityTracker : _.System.Object
{
    private ActivityTracker(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Threading.AsyncLocal<object>? m_current => Field<_.System.Threading.AsyncLocal<object>>();

    public bool m_checkedForEnable => Field<bool>();

    public static new ActivityTracker FromAddress(ulong address, DumpContext context)
        => new ActivityTracker(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ActivityTracker> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Diagnostics.Tracing.ActivityTracker"))
            yield return new ActivityTracker(addr, context);
    }

    public override string ToString() => $"ActivityTracker@0x{_objAddress:X}";
}
