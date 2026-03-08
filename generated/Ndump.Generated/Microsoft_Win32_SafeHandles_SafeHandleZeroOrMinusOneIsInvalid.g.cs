#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public class SafeHandleZeroOrMinusOneIsInvalid : _.System.Runtime.InteropServices.SafeHandle
{
    protected SafeHandleZeroOrMinusOneIsInvalid(ulong address, DumpContext context) : base(address, context) { }

    public static new SafeHandleZeroOrMinusOneIsInvalid FromAddress(ulong address, DumpContext context)
        => new SafeHandleZeroOrMinusOneIsInvalid(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SafeHandleZeroOrMinusOneIsInvalid> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid"))
            yield return new SafeHandleZeroOrMinusOneIsInvalid(addr, context);
    }

    public override string ToString() => $"SafeHandleZeroOrMinusOneIsInvalid@0x{_objAddress:X}";
}
