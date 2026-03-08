#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public partial class NonRandomizedStringEqualityComparer
{
    public sealed class OrdinalIgnoreCaseComparer : _.System.Collections.Generic.NonRandomizedStringEqualityComparer
    {
        private OrdinalIgnoreCaseComparer(ulong address, DumpContext context) : base(address, context) { }

        public static new OrdinalIgnoreCaseComparer FromAddress(ulong address, DumpContext context)
            => new OrdinalIgnoreCaseComparer(address, context);

        public static new global::System.Collections.Generic.IEnumerable<OrdinalIgnoreCaseComparer> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Collections.Generic.NonRandomizedStringEqualityComparer+OrdinalIgnoreCaseComparer"))
                yield return new OrdinalIgnoreCaseComparer(addr, context);
        }

        public override string ToString() => $"OrdinalIgnoreCaseComparer@0x{_objAddress:X}";
    }

}
