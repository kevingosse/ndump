#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class Decoder : _.System.Object
{
    protected Decoder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Text.DecoderFallback? _fallback => Field<_.System.Text.DecoderFallback>();

    public ulong _fallbackBuffer => RefAddress();

    public static new Decoder FromAddress(ulong address, DumpContext ctx)
        => new Decoder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Decoder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.Decoder"))
            yield return new Decoder(addr, ctx);
    }

    public override string ToString() => $"Decoder@0x{_objAddress:X}";
}
