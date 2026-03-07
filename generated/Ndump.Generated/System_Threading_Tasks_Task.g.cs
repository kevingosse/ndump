#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public partial class Task : _.System.Object
{
    protected Task(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int m_taskId => Field<int>();

    public _.System.Delegate? m_action => Field<_.System.Delegate>();

    public _.System.Object? m_stateObject => Field<_.System.Object>();

    public global::_.System.Object? m_taskScheduler => Field<global::_.System.Object>();

    public int m_stateFlags => Field<int>();

    public _.System.Object? m_continuationObject => Field<_.System.Object>();

    public global::_.System.Object? m_contingentProperties => Field<global::_.System.Object>();

    public static new Task FromAddress(ulong address, DumpContext ctx)
        => new Task(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Task> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.Task"))
            yield return new Task(addr, ctx);
    }

    public override string ToString() => $"Task@0x{_objAddress:X}";
}
