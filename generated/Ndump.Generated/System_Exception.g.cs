#nullable enable
using Ndump.Core;

namespace _.System;

public class Exception : _.System.Object
{
    protected Exception(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _exceptionMethod => _ctx.GetObjectAddress(_objAddress, "_exceptionMethod");

    public string? _message => _ctx.GetStringField(_objAddress, "_message");

    public ulong _data => _ctx.GetObjectAddress(_objAddress, "_data");

    public _.System.Exception? _innerException
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_innerException");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Exception ?? _.System.Exception.FromAddress(addr, _ctx);
        }
    }

    public string? _helpURL => _ctx.GetStringField(_objAddress, "_helpURL");

    public _.System.Object? _stackTrace
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_stackTrace");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public global::Ndump.Core.DumpArray<byte>? _watsonBuckets
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_watsonBuckets");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public string? _stackTraceString => _ctx.GetStringField(_objAddress, "_stackTraceString");

    public string? _remoteStackTraceString => _ctx.GetStringField(_objAddress, "_remoteStackTraceString");

    public string? _source => _ctx.GetStringField(_objAddress, "_source");

    public nuint _ipForWatsonBuckets => _ctx.GetFieldValue<nuint>(_objAddress, "_ipForWatsonBuckets");

    public nint _xptrs => _ctx.GetFieldValue<nint>(_objAddress, "_xptrs");

    public int _xcode => _ctx.GetFieldValue<int>(_objAddress, "_xcode");

    public int _HResult => _ctx.GetFieldValue<int>(_objAddress, "_HResult");

    public static new Exception FromAddress(ulong address, DumpContext ctx)
        => new Exception(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Exception> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Exception"))
            yield return new Exception(addr, ctx);
    }

    public override string ToString() => $"Exception@0x{_objAddress:X}";
}
