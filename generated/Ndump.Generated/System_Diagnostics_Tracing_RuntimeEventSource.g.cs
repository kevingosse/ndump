#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class RuntimeEventSource : _.System.Diagnostics.Tracing.EventSource
{
    private RuntimeEventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::_.System.Object? _gcHeapSizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen0GCCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen1GCCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen2GCCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen0BudgetCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _cpuTimeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _workingSetCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _threadPoolThreadCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _monitorContentionCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _threadPoolQueueCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _completedItemsCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _allocRateCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _timerCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _fragmentationCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _committedCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _exceptionCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gcTimeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _totalGcPauseTimeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen0SizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen1SizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _gen2SizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _lohSizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _pohSizeCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _assemblyCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _ilBytesJittedCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _methodsJittedCounter => Field<global::_.System.Object>();

    public global::_.System.Object? _jitTimeCounter => Field<global::_.System.Object>();

    public static new RuntimeEventSource FromAddress(ulong address, DumpContext ctx)
        => new RuntimeEventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeEventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.RuntimeEventSource"))
            yield return new RuntimeEventSource(addr, ctx);
    }

    public override string ToString() => $"RuntimeEventSource@0x{_objAddress:X}";
}
