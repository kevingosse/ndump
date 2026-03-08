#nullable enable
using Ndump.Core;

namespace _.System.Runtime.InteropServices;

public class SafeHandle : _.System.Runtime.ConstrainedExecution.CriticalFinalizerObject
{
    protected SafeHandle(ulong address, DumpContext context) : base(address, context) { }

    public nint handle => Field<nint>();

    public int _state => Field<int>();

    public bool _ownsHandle => Field<bool>();

    public bool _fullyInitialized => Field<bool>();

    public static new SafeHandle FromAddress(ulong address, DumpContext context)
        => new SafeHandle(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SafeHandle> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Runtime.InteropServices.SafeHandle"))
            yield return new SafeHandle(addr, context);
    }

    public override string ToString() => $"SafeHandle@0x{_objAddress:X}";
}
