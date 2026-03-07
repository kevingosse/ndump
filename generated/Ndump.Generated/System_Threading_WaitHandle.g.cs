#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public class WaitHandle : _.System.MarshalByRefObject
{
    protected WaitHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.Microsoft.Win32.SafeHandles.SafeWaitHandle? _waitHandle
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_waitHandle");
            return addr == 0 ? null : _.Microsoft.Win32.SafeHandles.SafeWaitHandle.FromAddress(addr, _ctx);
        }
    }

    public static new WaitHandle FromAddress(ulong address, DumpContext ctx)
        => new WaitHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<WaitHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.WaitHandle"))
            yield return new WaitHandle(addr, ctx);
    }

    public override string ToString() => $"WaitHandle@0x{_objAddress:X}";
}
