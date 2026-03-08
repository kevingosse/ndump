#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class Dictionary<T1, T2>
{
    public sealed class KeyCollection : global::_.System.Object
    {
        private KeyCollection(ulong address, DumpContext context) : base(address, context) { }

        public _.System.Collections.Generic.Dictionary<T1, T2>? _dictionary => Field<_.System.Collections.Generic.Dictionary<T1, T2>>();

        public static new KeyCollection FromAddress(ulong address, DumpContext context)
            => new KeyCollection(address, context);

        public override string ToString() => $"KeyCollection@0x{_objAddress:X}";
    }
}
