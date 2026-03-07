#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class EncoderFallback : _.System.Object
{
    protected EncoderFallback(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new EncoderFallback FromAddress(ulong address, DumpContext ctx)
        => new EncoderFallback(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EncoderFallback> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.EncoderFallback"))
            yield return new EncoderFallback(addr, ctx);
    }

    public override string ToString() => $"EncoderFallback@0x{_objAddress:X}";
}
