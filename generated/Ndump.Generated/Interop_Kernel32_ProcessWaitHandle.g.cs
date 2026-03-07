#nullable enable
using Ndump.Core;

namespace _;

public sealed class Interop_Kernel32_ProcessWaitHandle : _.System.Threading.WaitHandle
{
    private Interop_Kernel32_ProcessWaitHandle(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Interop_Kernel32_ProcessWaitHandle FromAddress(ulong address, DumpContext ctx)
        => new Interop_Kernel32_ProcessWaitHandle(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Interop_Kernel32_ProcessWaitHandle> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Interop+Kernel32+ProcessWaitHandle"))
            yield return new Interop_Kernel32_ProcessWaitHandle(addr, ctx);
    }

    public override string ToString() => $"Interop_Kernel32_ProcessWaitHandle@0x{_objAddress:X}";
}
