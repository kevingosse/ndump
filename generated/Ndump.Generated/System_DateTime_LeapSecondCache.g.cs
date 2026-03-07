#nullable enable
using Ndump.Core;

namespace _.System;

public partial class DateTime
{
    public sealed class LeapSecondCache : _.System.Object
    {
        private LeapSecondCache(ulong address, DumpContext ctx) : base(address, ctx) { }

        public ulong OSFileTimeTicksAtStartOfValidityWindow => Field<ulong>();

        public ulong DotnetDateDataAtStartOfValidityWindow => Field<ulong>();

        public static new LeapSecondCache FromAddress(ulong address, DumpContext ctx)
            => new LeapSecondCache(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<LeapSecondCache> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.DateTime+LeapSecondCache"))
                yield return new LeapSecondCache(addr, ctx);
        }

        public override string ToString() => $"LeapSecondCache@0x{_objAddress:X}";
    }

}
