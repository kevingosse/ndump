#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class Thread : _.System.Runtime.ConstrainedExecution.CriticalFinalizerObject
{
    private Thread(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _executionContext => _ctx.GetObjectAddress(_objAddress, "_executionContext");

    public ulong _synchronizationContext => _ctx.GetObjectAddress(_objAddress, "_synchronizationContext");

    public string? _name => _ctx.GetStringField(_objAddress, "_name");

    public ulong _startHelper => _ctx.GetObjectAddress(_objAddress, "_startHelper");

    public nint _DONT_USE_InternalThread => _ctx.GetFieldValue<nint>(_objAddress, "_DONT_USE_InternalThread");

    public int _priority => _ctx.GetFieldValue<int>(_objAddress, "_priority");

    public int _managedThreadId => _ctx.GetFieldValue<int>(_objAddress, "_managedThreadId");

    public bool _mayNeedResetForThreadPool => _ctx.GetFieldValue<bool>(_objAddress, "_mayNeedResetForThreadPool");

    public bool _isDead => _ctx.GetFieldValue<bool>(_objAddress, "_isDead");

    public bool _isThreadPool => _ctx.GetFieldValue<bool>(_objAddress, "_isThreadPool");

    public static new Thread FromAddress(ulong address, DumpContext ctx)
        => new Thread(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Thread> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Thread"))
            yield return new Thread(addr, ctx);
    }

    public override string ToString() => $"Thread@0x{_objAddress:X}";
}
