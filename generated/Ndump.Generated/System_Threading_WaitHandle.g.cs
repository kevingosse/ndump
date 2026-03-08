#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public class WaitHandle : _.System.MarshalByRefObject
{
    protected WaitHandle(ulong address, DumpContext context) : base(address, context) { }

    public _.Microsoft.Win32.SafeHandles.SafeWaitHandle? _waitHandle => Field<_.Microsoft.Win32.SafeHandles.SafeWaitHandle>();

    public static new WaitHandle FromAddress(ulong address, DumpContext context)
        => new WaitHandle(address, context);

    public static new global::System.Collections.Generic.IEnumerable<WaitHandle> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Threading.WaitHandle"))
            yield return new WaitHandle(addr, context);
    }

    public override string ToString() => $"WaitHandle@0x{_objAddress:X}";
}
