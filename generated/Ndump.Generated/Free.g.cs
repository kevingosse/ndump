#nullable enable
using Ndump.Core;

namespace _;

public sealed class Free : global::_.System.Object
{
    private Free(ulong address, DumpContext context) : base(address, context) { }

    public static new Free FromAddress(ulong address, DumpContext context)
        => new Free(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Free> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Free"))
            yield return new Free(addr, context);
    }

    public override string ToString() => $"Free@0x{_objAddress:X}";
}
