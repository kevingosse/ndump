#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class Task<T> : global::_.System.Object
{
    private Task(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int m_taskId => Field<int>();

    public _.System.Delegate? m_action => Field<_.System.Delegate>();

    public _.System.Object? m_stateObject => Field<_.System.Object>();

    public ulong m_taskScheduler => RefAddress();

    public int m_stateFlags => Field<int>();

    public _.System.Object? m_continuationObject => Field<_.System.Object>();

    public ulong m_contingentProperties => RefAddress();

    // ValueType field: m_result (object) — not yet supported

    public static new Task<T> FromAddress(ulong address, DumpContext ctx)
        => new Task<T>(address, ctx);

    public override string ToString() => $"Task@0x{_objAddress:X}";
}
