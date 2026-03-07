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
            var addr = _ctx.GetObjectAddress(_objAddress, "_stream");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.IO.Stream ?? _.System.IO.Stream.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Encoding? _encoding
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_encoding");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
        }
    }

    public _.System.Text.Decoder? _decoder
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_decoder");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Decoder ?? _.System.Text.Decoder.FromAddress(addr, _ctx);
        }
    }

    public global::Ndump.Core.DumpArray<byte>? _byteBuffer
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_byteBuffer");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public global::Ndump.Core.DumpArray<char>? _charBuffer
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_charBuffer");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<char>(addr, len, i => _ctx.GetArrayElementValue<char>(addr, i));
        }
    }

    public int _charPos => _ctx.GetFieldValue<int>(_objAddress, "_charPos");

    public int _charLen => _ctx.GetFieldValue<int>(_objAddress, "_charLen");

    public int _byteLen => _ctx.GetFieldValue<int>(_objAddress, "_byteLen");

    public int _bytePos => _ctx.GetFieldValue<int>(_objAddress, "_bytePos");

    public int _maxCharsPerBuffer => _ctx.GetFieldValue<int>(_objAddress, "_maxCharsPerBuffer");

    public bool _disposed => _ctx.GetFieldValue<bool>(_objAddress, "_disposed");

    public bool _detectEncoding => _ctx.GetFieldValue<bool>(_objAddress, "_detectEncoding");

    public bool _checkPreamble => _ctx.GetFieldValue<bool>(_objAddress, "_checkPreamble");

    public bool _isBlocked => _ctx.GetFieldValue<bool>(_objAddress, "_isBlocked");

    public bool _closable => _ctx.GetFieldValue<bool>(_objAddress, "_closable");

    public _.System.Threading.Tasks.Task? _asyncReadTask
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_asyncReadTask");
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
