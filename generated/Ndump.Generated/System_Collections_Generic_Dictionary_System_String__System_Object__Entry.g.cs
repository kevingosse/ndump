#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class Dictionary<T1, T2>
{
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
