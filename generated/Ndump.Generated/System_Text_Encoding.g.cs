#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class Encoding : _.System.Object
{
    protected Encoding(ulong address, DumpContext ctx) : base(address, ctx) { }

    public int _codePage => _ctx.GetFieldValue<int>(_objAddress, "_codePage");

    public ulong _dataItem => _ctx.GetObjectAddress(_objAddress, "_dataItem");

    public bool _isReadOnly => _ctx.GetFieldValue<bool>(_objAddress, "_isReadOnly");

    public _.System.Text.EncoderFallback? encoderFallback
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "encoderFallback");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.EncoderFallback ?? _.System.Text.EncoderFallback.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.DecoderFallback? decoderFallback
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "decoderFallback");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.DecoderFallback ?? _.System.Text.DecoderFallback.FromAddress(addr, _ctx);
        }
    }

    public static new Encoding FromAddress(ulong address, DumpContext ctx)
        => new Encoding(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Encoding> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.Encoding"))
            yield return new Encoding(addr, ctx);
    }

    public override string ToString() => $"Encoding@0x{_objAddress:X}";
}
