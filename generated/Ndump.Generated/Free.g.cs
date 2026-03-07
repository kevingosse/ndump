#nullable enable
using Ndump.Core;

namespace _;

public sealed class Free : global::_.System.Object
{
    private Free(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static Free FromAddress(ulong address, DumpContext ctx)
        => new Free(address, ctx);

    public static global::System.Collections.Generic.IEnumerable<Free> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Free"))
            yield return new Free(addr, ctx);
    }

    public override string ToString() => $"Free@0x{_objAddress:X}";
}
