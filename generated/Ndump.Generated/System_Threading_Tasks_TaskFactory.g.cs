#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class TaskFactory : _.System.Object
{
    private TaskFactory(ulong address, DumpContext ctx) : base(address, ctx) { }

    // ValueType field: m_defaultCancellationToken (object) — not yet supported

    public ulong m_defaultScheduler => RefAddress();

    public int m_defaultCreationOptions => Field<int>();

    public int m_defaultContinuationOptions => Field<int>();

    public static new TaskFactory FromAddress(ulong address, DumpContext ctx)
        => new TaskFactory(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<TaskFactory> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.TaskFactory"))
            yield return new TaskFactory(addr, ctx);
    }

    public override string ToString() => $"TaskFactory@0x{_objAddress:X}";
}
