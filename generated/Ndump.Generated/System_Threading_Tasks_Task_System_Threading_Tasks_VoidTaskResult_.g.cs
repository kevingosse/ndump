#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class Task_System_Threading_Tasks_VoidTaskResult_ : _.System.Threading.Tasks.Task
{
    private Task_System_Threading_Tasks_VoidTaskResult_(ulong address, DumpContext ctx) : base(address, ctx) { }

    // ValueType field: m_result (object) — not yet supported

    public static new Task_System_Threading_Tasks_VoidTaskResult_ FromAddress(ulong address, DumpContext ctx)
        => new Task_System_Threading_Tasks_VoidTaskResult_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Task_System_Threading_Tasks_VoidTaskResult_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>"))
            yield return new Task_System_Threading_Tasks_VoidTaskResult_(addr, ctx);
    }

    public override string ToString() => $"Task_System_Threading_Tasks_VoidTaskResult_@0x{_objAddress:X}";
}
