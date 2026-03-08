#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class Encoding : _.System.Object
{
    protected Encoding(ulong address, DumpContext context) : base(address, context) { }

    public int _codePage => Field<int>();

    public global::_.System.Object? _dataItem => Field<global::_.System.Object>();

    public bool _isReadOnly => Field<bool>();

    public _.System.Text.EncoderFallback? encoderFallback => Field<_.System.Text.EncoderFallback>();

    public _.System.Text.DecoderFallback? decoderFallback => Field<_.System.Text.DecoderFallback>();

    public static new Encoding FromAddress(ulong address, DumpContext context)
        => new Encoding(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Encoding> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.Encoding"))
            yield return new Encoding(addr, context);
    }

    public override string ToString() => $"Encoding@0x{_objAddress:X}";
}
