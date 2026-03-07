#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer
{
    public sealed class OrdinalIgnoreCaseComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
    {
        private OrdinalIgnoreCaseComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

        public static new OrdinalIgnoreCaseComparer FromAddress(ulong address, DumpContext ctx)
            => new OrdinalIgnoreCaseComparer(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<OrdinalIgnoreCaseComparer> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalIgnoreCaseComparer"))
                yield return new OrdinalIgnoreCaseComparer(addr, ctx);
        }

        public override string ToString() => $"OrdinalIgnoreCaseComparer@0x{_objAddress:X}";
    }

}
