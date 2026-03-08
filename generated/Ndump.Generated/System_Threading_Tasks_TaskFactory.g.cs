#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class TaskFactory : _.System.Object
{
    private TaskFactory(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Threading.CancellationToken m_defaultCancellationToken => StructField<_.System.Threading.CancellationToken>("System.Threading.CancellationToken");

    public global::_.System.Object? m_defaultScheduler => Field<global::_.System.Object>();

    public int m_defaultCreationOptions => Field<int>();

    public int m_defaultContinuationOptions => Field<int>();

    public static new TaskFactory FromAddress(ulong address, DumpContext context)
        => new TaskFactory(address, context);

    public static new global::System.Collections.Generic.IEnumerable<TaskFactory> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Threading.Tasks.TaskFactory"))
            yield return new TaskFactory(addr, context);
    }

    public override string ToString() => $"TaskFactory@0x{_objAddress:X}";
}
