#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class List<T> : global::_.System.Object
{
    private List(ulong address, DumpContext ctx) : base(address, ctx) { }

    // Array field: _items (T[]) — element type not supported

    public int _size => Field<int>();

    public int _version => Field<int>();

    public static new List<T> FromAddress(ulong address, DumpContext ctx)
        => new List<T>(address, ctx);

    public override string ToString() => $"List@0x{_objAddress:X}";
}
