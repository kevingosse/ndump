#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class RuntimeEventSource : _.System.Diagnostics.Tracing.EventSource
{
    private RuntimeEventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _gcHeapSizeCounter => RefAddress();

    public ulong _gen0GCCounter => RefAddress();

    public ulong _gen1GCCounter => RefAddress();

    public ulong _gen2GCCounter => RefAddress();

    public ulong _gen0BudgetCounter => RefAddress();

    public ulong _cpuTimeCounter => RefAddress();

    public ulong _workingSetCounter => RefAddress();

    public ulong _threadPoolThreadCounter => RefAddress();

    public ulong _monitorContentionCounter => RefAddress();

    public ulong _threadPoolQueueCounter => RefAddress();

    public ulong _completedItemsCounter => RefAddress();

    public ulong _allocRateCounter => RefAddress();

    public ulong _timerCounter => RefAddress();

    public ulong _fragmentationCounter => RefAddress();

    public ulong _committedCounter => RefAddress();

    public ulong _exceptionCounter => RefAddress();

    public ulong _gcTimeCounter => RefAddress();

    public ulong _totalGcPauseTimeCounter => RefAddress();

    public ulong _gen0SizeCounter => RefAddress();

    public ulong _gen1SizeCounter => RefAddress();

    public ulong _gen2SizeCounter => RefAddress();

    public ulong _lohSizeCounter => RefAddress();

    public ulong _pohSizeCounter => RefAddress();

    public ulong _assemblyCounter => RefAddress();

    public ulong _ilBytesJittedCounter => RefAddress();

    public ulong _methodsJittedCounter => RefAddress();

    public ulong _jitTimeCounter => RefAddress();

    public static new RuntimeEventSource FromAddress(ulong address, DumpContext ctx)
        => new RuntimeEventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeEventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.RuntimeEventSource"))
            yield return new RuntimeEventSource(addr, ctx);
    }

    public override string ToString() => $"RuntimeEventSource@0x{_objAddress:X}";
}
