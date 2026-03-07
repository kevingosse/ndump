#nullable enable
using Ndump.Core;

namespace _.Microsoft.Win32.SafeHandles;

public class SafeHandleZeroOrMinusOneIsInvalid : _.System.Runtime.InteropServices.SafeHandle
{
    protected SafeHandleZeroOrMinusOneIsInvalid(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new SafeHandleZeroOrMinusOneIsInvalid FromAddress(ulong address, DumpContext ctx)
        => new SafeHandleZeroOrMinusOneIsInvalid(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SafeHandleZeroOrMinusOneIsInvalid> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid"))
            yield return new SafeHandleZeroOrMinusOneIsInvalid(addr, ctx);
    }

    public override string ToString() => $"SafeHandleZeroOrMinusOneIsInvalid@0x{_objAddress:X}";
}
