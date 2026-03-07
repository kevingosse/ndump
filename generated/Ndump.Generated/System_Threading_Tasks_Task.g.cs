#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public partial class Task : _.System.Object
{
    protected Task(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int m_taskId => Field<int>();

    public _.System.Delegate? m_action
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Delegate ?? _.System.Delegate.FromAddress(addr, _ctx);
        }
    }

    public _.System.Object? m_stateObject
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public ulong m_taskScheduler => RefAddress();

    public int m_stateFlags => Field<int>();

    public _.System.Object? m_continuationObject
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public ulong m_contingentProperties => RefAddress();

    public static new Task FromAddress(ulong address, DumpContext ctx)
        => new Task(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Task> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.Task"))
            yield return new Task(addr, ctx);
    }

    public override string ToString() => $"Task@0x{_objAddress:X}";
}
