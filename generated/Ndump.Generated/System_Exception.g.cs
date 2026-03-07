#nullable enable
using Ndump.Core;

namespace _.System;

public class Exception : _.System.Object
{
    protected Exception(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::_.System.Object? _exceptionMethod => Field<global::_.System.Object>();

    public string? _message => Field<string>();

    public global::_.System.Object? _data => Field<global::_.System.Object>();

    public _.System.Exception? _innerException => Field<_.System.Exception>();

    public string? _helpURL => Field<string>();

    public _.System.Object? _stackTrace => Field<_.System.Object>();

    // Array field: _watsonBuckets (object) — element type not supported

    public string? _stackTraceString => Field<string>();

    public string? _remoteStackTraceString => Field<string>();

    public string? _source => Field<string>();

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
