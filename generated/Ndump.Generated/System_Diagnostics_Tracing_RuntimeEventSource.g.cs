#nullable enable
using Ndump.Core;

namespace _.System.Diagnostics.Tracing;

public sealed class RuntimeEventSource : _.System.Diagnostics.Tracing.EventSource
{
    private RuntimeEventSource(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _gcHeapSizeCounter => _ctx.GetObjectAddress(_objAddress, "_gcHeapSizeCounter");

    public ulong _gen0GCCounter => _ctx.GetObjectAddress(_objAddress, "_gen0GCCounter");

    public ulong _gen1GCCounter => _ctx.GetObjectAddress(_objAddress, "_gen1GCCounter");

    public ulong _gen2GCCounter => _ctx.GetObjectAddress(_objAddress, "_gen2GCCounter");

    public ulong _gen0BudgetCounter => _ctx.GetObjectAddress(_objAddress, "_gen0BudgetCounter");

    public ulong _cpuTimeCounter => _ctx.GetObjectAddress(_objAddress, "_cpuTimeCounter");

    public ulong _workingSetCounter => _ctx.GetObjectAddress(_objAddress, "_workingSetCounter");

    public ulong _threadPoolThreadCounter => _ctx.GetObjectAddress(_objAddress, "_threadPoolThreadCounter");

    public ulong _monitorContentionCounter => _ctx.GetObjectAddress(_objAddress, "_monitorContentionCounter");

    public ulong _threadPoolQueueCounter => _ctx.GetObjectAddress(_objAddress, "_threadPoolQueueCounter");

    public ulong _completedItemsCounter => _ctx.GetObjectAddress(_objAddress, "_completedItemsCounter");

    public ulong _allocRateCounter => _ctx.GetObjectAddress(_objAddress, "_allocRateCounter");

    public ulong _timerCounter => _ctx.GetObjectAddress(_objAddress, "_timerCounter");

    public ulong _fragmentationCounter => _ctx.GetObjectAddress(_objAddress, "_fragmentationCounter");

    public ulong _committedCounter => _ctx.GetObjectAddress(_objAddress, "_committedCounter");

    public ulong _exceptionCounter => _ctx.GetObjectAddress(_objAddress, "_exceptionCounter");

    public ulong _gcTimeCounter => _ctx.GetObjectAddress(_objAddress, "_gcTimeCounter");

    public ulong _totalGcPauseTimeCounter => _ctx.GetObjectAddress(_objAddress, "_totalGcPauseTimeCounter");

    public ulong _gen0SizeCounter => _ctx.GetObjectAddress(_objAddress, "_gen0SizeCounter");

    public ulong _gen1SizeCounter => _ctx.GetObjectAddress(_objAddress, "_gen1SizeCounter");

    public ulong _gen2SizeCounter => _ctx.GetObjectAddress(_objAddress, "_gen2SizeCounter");

    public ulong _lohSizeCounter => _ctx.GetObjectAddress(_objAddress, "_lohSizeCounter");

    public ulong _pohSizeCounter => _ctx.GetObjectAddress(_objAddress, "_pohSizeCounter");

    public ulong _assemblyCounter => _ctx.GetObjectAddress(_objAddress, "_assemblyCounter");

    public ulong _ilBytesJittedCounter => _ctx.GetObjectAddress(_objAddress, "_ilBytesJittedCounter");

    public ulong _methodsJittedCounter => _ctx.GetObjectAddress(_objAddress, "_methodsJittedCounter");

    public ulong _jitTimeCounter => _ctx.GetObjectAddress(_objAddress, "_jitTimeCounter");

    public static new RuntimeEventSource FromAddress(ulong address, DumpContext ctx)
        => new RuntimeEventSource(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeEventSource> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Diagnostics.Tracing.RuntimeEventSource"))
            yield return new RuntimeEventSource(addr, ctx);
    }

    public override string ToString() => $"RuntimeEventSource@0x{_objAddress:X}";
}
