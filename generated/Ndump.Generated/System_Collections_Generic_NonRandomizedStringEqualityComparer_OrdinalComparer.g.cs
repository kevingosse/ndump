#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer
{
    public sealed class OrdinalComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
    {
        private OrdinalComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

        public static new OrdinalComparer FromAddress(ulong address, DumpContext ctx)
            => new OrdinalComparer(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<OrdinalComparer> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalComparer"))
                yield return new OrdinalComparer(addr, ctx);
        }

        public override string ToString() => $"OrdinalComparer@0x{_objAddress:X}";
    }

}
