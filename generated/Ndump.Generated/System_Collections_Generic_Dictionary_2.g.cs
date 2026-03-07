#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class Dictionary<T1, T2> : global::_.System.Object
{
    private Dictionary(ulong address, DumpContext ctx) : base(address, ctx) { }

    public global::Ndump.Core.DumpArray<int>? _buckets
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<int>(addr, len, i => _ctx.GetArrayElementValue<int>(addr, i));
        }
    }

    public global::Ndump.Core.DumpArray<Entry>? _entries
    {
        get
        {
            var addr = RefAddress();
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<Entry>(addr, len, i => Entry.FromArrayElement(_ctx.GetArrayStructElementAddress(addr, i), _ctx, addr, i));
        }
    }

    public ulong _fastModMultiplier => Field<ulong>();

    public int _count => Field<int>();

    public int _freeList => Field<int>();

    public int _freeCount => Field<int>();

    public int _version => Field<int>();

    public ulong _comparer => RefAddress();

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
