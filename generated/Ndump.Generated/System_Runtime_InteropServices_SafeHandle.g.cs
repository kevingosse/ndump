#nullable enable
using Ndump.Core;

namespace _.System.Runtime.InteropServices;

public class SafeHandle : _.System.Runtime.ConstrainedExecution.CriticalFinalizerObject
{
    protected SafeHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public nint handle => _ctx.GetFieldValue<nint>(_objAddress, "handle");

    public int _state => _ctx.GetFieldValue<int>(_objAddress, "_state");

    public bool _ownsHandle => _ctx.GetFieldValue<bool>(_objAddress, "_ownsHandle");

    public bool _fullyInitialized => _ctx.GetFieldValue<bool>(_objAddress, "_fullyInitialized");

    public static new SafeHandle FromAddress(ulong address, DumpContext ctx)
        => new SafeHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Runtime.InteropServices.SafeHandle"))
            yield return new SafeHandle(addr, ctx);
    }

    public override string ToString() => $"SafeHandle@0x{_objAddress:X}";
}
