#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeWaitHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeWaitHandle(ulong address, DumpContext context) : base(address, context) { }

    public static new SafeWaitHandle FromAddress(ulong address, DumpContext context)
        => new SafeWaitHandle(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SafeWaitHandle> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeWaitHandle"))
            yield return new SafeWaitHandle(addr, context);
    }

    public override string ToString() => $"SafeWaitHandle@0x{_objAddress:X}";
}
