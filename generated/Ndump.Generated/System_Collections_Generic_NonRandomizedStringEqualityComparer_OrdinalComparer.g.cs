#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer
{
    public sealed class OrdinalComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
    {
        private OrdinalComparer(ulong address, DumpContext context) : base(address, context) { }

        public static new OrdinalComparer FromAddress(ulong address, DumpContext context)
            => new OrdinalComparer(address, context);

        public static new global::System.Collections.Generic.IEnumerable<OrdinalComparer> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalComparer"))
                yield return new OrdinalComparer(addr, context);
        }

        public override string ToString() => $"OrdinalComparer@0x{_objAddress:X}";
    }

}
