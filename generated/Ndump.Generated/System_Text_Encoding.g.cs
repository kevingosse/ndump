#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class Encoding : _.System.Object
{
    protected Encoding(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _codePage => Field<int>();

    public ulong _dataItem => RefAddress();

    public bool _isReadOnly => Field<bool>();

    public _.System.Text.EncoderFallback? encoderFallback => Field<_.System.Text.EncoderFallback>();

    public _.System.Text.DecoderFallback? decoderFallback => Field<_.System.Text.DecoderFallback>();

    public static new Encoding FromAddress(ulong address, DumpContext ctx)
        => new Encoding(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Encoding> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.Encoding"))
            yield return new Encoding(addr, ctx);
    }

    public override string ToString() => $"Encoding@0x{_objAddress:X}";
}
