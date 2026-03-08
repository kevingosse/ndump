#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class Decoder : _.System.Object
{
    protected Decoder(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Text.DecoderFallback? _fallback => Field<_.System.Text.DecoderFallback>();

    public global::_.System.Object? _fallbackBuffer => Field<global::_.System.Object>();

    public static new Decoder FromAddress(ulong address, DumpContext context)
        => new Decoder(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Decoder> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.Decoder"))
            yield return new Decoder(addr, context);
    }

    public override string ToString() => $"Decoder@0x{_objAddress:X}";
}
