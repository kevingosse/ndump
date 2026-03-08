#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class Task<T> : global::_.System.Object
{
    private Task(ulong address, DumpContext context) : base(address, context) { }

    public int m_taskId => Field<int>();

    public _.System.Delegate? m_action => Field<_.System.Delegate>();

    public _.System.Object? m_stateObject => Field<_.System.Object>();

    public global::_.System.Object? m_taskScheduler => Field<global::_.System.Object>();

    public int m_stateFlags => Field<int>();

    public _.System.Object? m_continuationObject => Field<_.System.Object>();

    public global::_.System.Object? m_contingentProperties => Field<global::_.System.Object>();

    public _.System.Threading.Tasks.VoidTaskResult m_result => StructField<_.System.Threading.Tasks.VoidTaskResult>("System.Threading.Tasks.VoidTaskResult");

    public static new Task<T> FromAddress(ulong address, DumpContext context)
        => new Task<T>(address, context);

    public override string ToString() => $"Task@0x{_objAddress:X}";
}
