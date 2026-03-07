#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeWaitHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeWaitHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new SafeWaitHandle FromAddress(ulong address, DumpContext ctx)
        => new SafeWaitHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeWaitHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeWaitHandle"))
            yield return new SafeWaitHandle(addr, ctx);
    }

    public override string ToString() => $"SafeWaitHandle@0x{_objAddress:X}";
}
