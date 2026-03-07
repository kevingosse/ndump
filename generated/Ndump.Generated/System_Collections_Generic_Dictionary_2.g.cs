#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class Dictionary<T1, T2> : global::_.System.Object
{
    private Dictionary(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::Ndump.Core.DumpArray<int>? _buckets => ArrayField<int>();

    public global::Ndump.Core.DumpArray<Entry>? _entries => ArrayField<Entry>();

    public ulong _fastModMultiplier => Field<ulong>();

    public int _count => Field<int>();

    public int _freeList => Field<int>();

    public int _freeCount => Field<int>();

    public int _version => Field<int>();

    public global::_.System.Object? _comparer => Field<global::_.System.Object>();

    // Unknown field: _keys (object)

    // Unknown field: _values (object)

    public static new Dictionary<T1, T2> FromAddress(ulong address, DumpContext ctx)
        => new Dictionary<T1, T2>(address, ctx);

    public override string ToString() => $"Dictionary@0x{_objAddress:X}";

    public sealed class Entry : global::_.System.Object
    {
        private Entry(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }

        public uint hashCode => Field<uint>();

        public int next => Field<int>();

        public T1? key => Field<T1>();

        public T2? value => Field<T2>();

        public static Entry FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
            => new Entry(address, ctx, arrayAddr, arrayIndex);

        public override string ToString() => $"Entry@0x{_objAddress:X}";
    }
}
