#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class DecoderFallback : _.System.Object
{
    protected DecoderFallback(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new DecoderFallback FromAddress(ulong address, DumpContext ctx)
        => new DecoderFallback(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<DecoderFallback> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.DecoderFallback"))
            yield return new DecoderFallback(addr, ctx);
    }

    public override string ToString() => $"DecoderFallback@0x{_objAddress:X}";
}
