#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class Dictionary<T1, T2>
{
    public sealed class Entry : global::_.System.Object
    {
        private Entry(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

        public uint hashCode => Field<uint>();

        public int next => Field<int>();

        public T1? key => Field<T1>();

        public T2? value => Field<T2>();

        public static Entry FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
            => new Entry(address, ctx, interiorTypeName);

        public override string ToString() => $"Entry@0x{_objAddress:X}";
    }
}
