#nullable enable
using Ndump.Core;

namespace _.System;

public class Exception : _.System.Object
{
    protected Exception(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _exceptionMethod => Field<global::_.System.Object>();

    public string? _message => Field<string>();

    public global::_.System.Object? _data => Field<global::_.System.Object>();

    public _.System.Exception? _innerException => Field<_.System.Exception>();

    public string? _helpURL => Field<string>();

    public _.System.Object? _stackTrace => Field<_.System.Object>();

    public global::Ndump.Core.DumpArray<byte>? _watsonBuckets => ArrayField<byte>();

    public string? _stackTraceString => Field<string>();

    public string? _remoteStackTraceString => Field<string>();

    public string? _source => Field<string>();

    public nuint _ipForWatsonBuckets => Field<nuint>();

    public nint _xptrs => Field<nint>();

    public int _xcode => Field<int>();

    public int _HResult => Field<int>();

    public static new Exception FromAddress(ulong address, DumpContext context)
        => new Exception(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Exception> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Exception"))
            yield return new Exception(addr, context);
    }

    public override string ToString() => $"Exception@0x{_objAddress:X}";
}
