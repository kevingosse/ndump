#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class Func_System_Diagnostics_Tracing_EventSource_ : _.System.MulticastDelegate
{
    private Func_System_Diagnostics_Tracing_EventSource_(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Func_System_Diagnostics_Tracing_EventSource_ FromAddress(ulong address, DumpContext ctx)
        => new Func_System_Diagnostics_Tracing_EventSource_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Func_System_Diagnostics_Tracing_EventSource_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Func<System.Diagnostics.Tracing.EventSource>"))
            yield return new Func_System_Diagnostics_Tracing_EventSource_(addr, ctx);
    }

    public override string ToString() => $"Func_System_Diagnostics_Tracing_EventSource_@0x{_objAddress:X}";
}
