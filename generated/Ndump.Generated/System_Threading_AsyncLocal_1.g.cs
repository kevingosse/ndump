#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class AsyncLocal<T> : global::_.System.Object
{
    private AsyncLocal(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::_.System.Object? _valueChangedHandler => Field<global::_.System.Object>();

    public static new AsyncLocal<T> FromAddress(ulong address, DumpContext ctx)
        => new AsyncLocal<T>(address, ctx);

    public override string ToString() => $"AsyncLocal@0x{_objAddress:X}";
}
