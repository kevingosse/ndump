#nullable enable
using Ndump.Core;

namespace _.System.IO;

public sealed class StreamReader : _.System.IO.TextReader
{
    private StreamReader(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.IO.Stream? _stream => Field<_.System.IO.Stream>();

    public _.System.Text.Encoding? _encoding => Field<_.System.Text.Encoding>();

    public _.System.Text.Decoder? _decoder => Field<_.System.Text.Decoder>();

    public global::Ndump.Core.DumpArray<byte>? _byteBuffer
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public global::Ndump.Core.DumpArray<char>? _charBuffer
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<char>(addr, len, i => _ctx.GetArrayElementValue<char>(addr, i));
        }
    }

    public int _charPos => Field<int>();

    public int _charLen => Field<int>();

    public int _byteLen => Field<int>();

    public int _bytePos => Field<int>();

    public int _maxCharsPerBuffer => Field<int>();

    public bool _disposed => Field<bool>();

    public bool _detectEncoding => Field<bool>();

    public bool _checkPreamble => Field<bool>();

    public bool _isBlocked => Field<bool>();

    public bool _closable => Field<bool>();

    public _.System.Threading.Tasks.Task? _asyncReadTask => Field<_.System.Threading.Tasks.Task>();

    public static new StreamReader FromAddress(ulong address, DumpContext ctx)
        => new StreamReader(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StreamReader> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.StreamReader"))
            yield return new StreamReader(addr, ctx);
    }

    public override string ToString() => $"StreamReader@0x{_objAddress:X}";
}
