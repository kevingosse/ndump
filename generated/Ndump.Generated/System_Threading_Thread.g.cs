#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class Thread : _.System.Runtime.ConstrainedExecution.CriticalFinalizerObject
{
    private Thread(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _executionContext => Field<global::_.System.Object>();

    public global::_.System.Object? _synchronizationContext => Field<global::_.System.Object>();

    public string? _name => Field<string>();

    public global::_.System.Object? _startHelper => Field<global::_.System.Object>();

    public nint _DONT_USE_InternalThread => Field<nint>();

    public int _priority => Field<int>();

    public int _managedThreadId => Field<int>();

    public bool _mayNeedResetForThreadPool => Field<bool>();

    public bool _isDead => Field<bool>();

    public bool _isThreadPool => Field<bool>();

    public static new Thread FromAddress(ulong address, DumpContext context)
        => new Thread(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Thread> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Threading.Thread"))
            yield return new Thread(addr, context);
    }

    public override string ToString() => $"Thread@0x{_objAddress:X}";
}
