#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class AsyncLocal<T> : global::_.System.Object
{
    private AsyncLocal(ulong address, DumpContext context) : base(address, context) { }

    public global::_.System.Object? _valueChangedHandler => Field<global::_.System.Object>();

    public static new AsyncLocal<T> FromAddress(ulong address, DumpContext context)
        => new AsyncLocal<T>(address, context);

    public override string ToString() => $"AsyncLocal@0x{_objAddress:X}";
}
