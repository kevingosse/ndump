#nullable enable
using Ndump.Core;

namespace _.System;

public class Exception : _.System.Object
{
    protected Exception(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _exceptionMethod => RefAddress();

    public string? _message => StringField();

    public ulong _data => RefAddress();

    public _.System.Exception? _innerException
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Exception ?? _.System.Exception.FromAddress(addr, _ctx);
        }
    }

    public string? _helpURL => StringField();

    public _.System.Object? _stackTrace
    {
        get
        {
            var addr = RefAddress();
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public global::Ndump.Core.DumpArray<byte>? _watsonBuckets
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<byte>(addr, len, i => _ctx.GetArrayElementValue<byte>(addr, i));
        }
    }

    public string? _stackTraceString => StringField();

    public string? _remoteStackTraceString => StringField();

    public string? _source => StringField();

    public nuint _ipForWatsonBuckets => Field<nuint>();

    public nint _xptrs => Field<nint>();

    public int _xcode => Field<int>();

    public int _HResult => Field<int>();

    public static new Exception FromAddress(ulong address, DumpContext ctx)
        => new Exception(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Exception> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Exception"))
            yield return new Exception(addr, ctx);
    }

    public override string ToString() => $"Exception@0x{_objAddress:X}";
}
