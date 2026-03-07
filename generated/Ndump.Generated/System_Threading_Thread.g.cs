#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class Thread : _.System.Runtime.ConstrainedExecution.CriticalFinalizerObject
{
    private Thread(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _executionContext => RefAddress();

    public ulong _synchronizationContext => RefAddress();

    public string? _name => StringField();

    public ulong _startHelper => RefAddress();

    public nint _DONT_USE_InternalThread => Field<nint>();

    public int _priority => Field<int>();

    public int _managedThreadId => Field<int>();

    public bool _mayNeedResetForThreadPool => Field<bool>();

    public bool _isDead => Field<bool>();

    public bool _isThreadPool => Field<bool>();

    public static new Thread FromAddress(ulong address, DumpContext ctx)
        => new Thread(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Thread> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Thread"))
            yield return new Thread(addr, ctx);
    }

    public override string ToString() => $"Thread@0x{_objAddress:X}";
}
