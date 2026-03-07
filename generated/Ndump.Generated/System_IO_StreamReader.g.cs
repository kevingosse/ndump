#nullable enable
using Ndump.Core;

namespace _.System.IO;

public sealed class StreamReader : _.System.IO.TextReader
{
    private StreamReader(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.IO.Stream? _stream
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.IO.Stream ?? _.System.IO.Stream.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? _encoding
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Decoder? _decoder
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Decoder ?? _.System.Text.Decoder.FromAddress(addr, _ctx);
        }
    }

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

    public _.System.Threading.Tasks.Task? _asyncReadTask
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Threading.Tasks.Task ?? _.System.Threading.Tasks.Task.FromAddress(addr, _ctx);
        }
    }

    public static new StreamReader FromAddress(ulong address, DumpContext ctx)
        => new StreamReader(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StreamReader> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.StreamReader"))
            yield return new StreamReader(addr, ctx);
    }

    public override string ToString() => $"StreamReader@0x{_objAddress:X}";
}
