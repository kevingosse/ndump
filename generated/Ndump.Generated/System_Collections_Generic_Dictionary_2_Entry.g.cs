#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class Dictionary<T1, T2>
{
    public sealed class Entry : global::_.System.Object, global::Ndump.Core.IProxy<Entry>
    {
        private Entry(ulong address, DumpContext context) : base(address, context) { }
        private Entry(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

        public uint hashCode => Field<uint>();

        public int next => Field<int>();

        public T1? key => Field<T1>();

        public T2? value => Field<T2>();

        public static new Entry FromAddress(ulong address, DumpContext context)
            => new Entry(address, context);

        public static Entry FromInterior(ulong address, DumpContext context, string interiorTypeName)
            => new Entry(address, context, interiorTypeName);

        public override string ToString() => $"Entry@0x{_objAddress:X}";
    }
}
