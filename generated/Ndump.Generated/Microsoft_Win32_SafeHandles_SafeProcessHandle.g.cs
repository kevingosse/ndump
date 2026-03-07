#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeProcessHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeProcessHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new SafeProcessHandle FromAddress(ulong address, DumpContext ctx)
        => new SafeProcessHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeProcessHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeProcessHandle"))
            yield return new SafeProcessHandle(addr, ctx);
    }

    public override string ToString() => $"SafeProcessHandle@0x{_objAddress:X}";
}
