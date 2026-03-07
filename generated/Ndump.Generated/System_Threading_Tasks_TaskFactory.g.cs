#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class TaskFactory : _.System.Object
{
    private TaskFactory(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Threading.CancellationToken m_defaultCancellationToken => StructField<_.System.Threading.CancellationToken>("System.Threading.CancellationToken");

    public global::_.System.Object? m_defaultScheduler => Field<global::_.System.Object>();

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
