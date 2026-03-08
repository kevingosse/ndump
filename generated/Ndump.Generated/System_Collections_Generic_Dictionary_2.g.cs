#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class Dictionary<T1, T2> : global::_.System.Object
{
    private Dictionary(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::Ndump.Core.DumpArray<int>? _buckets => ArrayField<int>();

    public global::Ndump.Core.DumpArray<Entry>? _entries => StructArrayField<Entry>();

    public ulong _fastModMultiplier => Field<ulong>();

    public int _count => Field<int>();

    public int _freeList => Field<int>();

    public int _freeCount => Field<int>();

    public int _version => Field<int>();

    public global::_.System.Object? _comparer => Field<global::_.System.Object>();

    public KeyCollection? _keys => Field<KeyCollection>();

    public global::_.System.Object? _values => Field<global::_.System.Object>();

    public static new Dictionary<T1, T2> FromAddress(ulong address, DumpContext ctx)
        => new Dictionary<T1, T2>(address, ctx);

    public override string ToString() => $"Dictionary@0x{_objAddress:X}";
}
