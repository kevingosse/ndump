#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class WeakReference_System_Diagnostics_Tracing_EventProvider_ : _.System.Object
{
    private WeakReference_System_Diagnostics_Tracing_EventProvider_(ulong address, DumpContext ctx) : base(address, ctx) { }

    public nint _taggedHandle => Field<nint>();

    public static new WeakReference_System_Diagnostics_Tracing_EventProvider_ FromAddress(ulong address, DumpContext ctx)
        => new WeakReference_System_Diagnostics_Tracing_EventProvider_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<WeakReference_System_Diagnostics_Tracing_EventProvider_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.WeakReference<System.Diagnostics.Tracing.EventProvider>"))
            yield return new WeakReference_System_Diagnostics_Tracing_EventProvider_(addr, ctx);
    }

    public override string ToString() => $"WeakReference_System_Diagnostics_Tracing_EventProvider_@0x{_objAddress:X}";
}
