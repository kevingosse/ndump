#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class List_System_WeakReference_System_Diagnostics_Tracing_EventSource__ : _.System.Object
{
    private List_System_WeakReference_System_Diagnostics_Tracing_EventSource__(ulong address, DumpContext ctx) : base(address, ctx) { }

    // Array field: _items (T[]) — element type not supported

    public int _size => Field<int>();

    public int _version => Field<int>();

    public static new List_System_WeakReference_System_Diagnostics_Tracing_EventSource__ FromAddress(ulong address, DumpContext ctx)
        => new List_System_WeakReference_System_Diagnostics_Tracing_EventSource__(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<List_System_WeakReference_System_Diagnostics_Tracing_EventSource__> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.List<System.WeakReference<System.Diagnostics.Tracing.EventSource>>"))
            yield return new List_System_WeakReference_System_Diagnostics_Tracing_EventSource__(addr, ctx);
    }

    public override string ToString() => $"List_System_WeakReference_System_Diagnostics_Tracing_EventSource__@0x{_objAddress:X}";
}
