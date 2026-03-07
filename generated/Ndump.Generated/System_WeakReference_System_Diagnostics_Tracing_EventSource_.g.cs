#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class WeakReference_System_Diagnostics_Tracing_EventSource_ : _.System.Object
{
    private WeakReference_System_Diagnostics_Tracing_EventSource_(ulong address, DumpContext ctx) : base(address, ctx) { }

    public nint _taggedHandle => _ctx.GetFieldValue<nint>(_objAddress, "_taggedHandle");

    public static new WeakReference_System_Diagnostics_Tracing_EventSource_ FromAddress(ulong address, DumpContext ctx)
        => new WeakReference_System_Diagnostics_Tracing_EventSource_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<WeakReference_System_Diagnostics_Tracing_EventSource_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.WeakReference<System.Diagnostics.Tracing.EventSource>"))
            yield return new WeakReference_System_Diagnostics_Tracing_EventSource_(addr, ctx);
    }

    public override string ToString() => $"WeakReference_System_Diagnostics_Tracing_EventSource_@0x{_objAddress:X}";
}
