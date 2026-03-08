#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public sealed class SafeProcessHandle : _.Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeProcessHandle(ulong address, DumpContext context) : base(address, context) { }

    public static new SafeProcessHandle FromAddress(ulong address, DumpContext context)
        => new SafeProcessHandle(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SafeProcessHandle> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeProcessHandle"))
            yield return new SafeProcessHandle(addr, context);
    }

    public override string ToString() => $"SafeProcessHandle@0x{_objAddress:X}";
}
