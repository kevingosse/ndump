#nullable enable
using Ndump.Core;

namespace _.System;

public partial class DateTime
{
    public sealed class LeapSecondCache : _.System.Object
    {
        private LeapSecondCache(ulong address, DumpContext context) : base(address, context) { }

        public ulong OSFileTimeTicksAtStartOfValidityWindow => Field<ulong>();

        public ulong DotnetDateDataAtStartOfValidityWindow => Field<ulong>();

        public static new LeapSecondCache FromAddress(ulong address, DumpContext context)
            => new LeapSecondCache(address, context);

        public static new global::System.Collections.Generic.IEnumerable<LeapSecondCache> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.DateTime+LeapSecondCache"))
                yield return new LeapSecondCache(addr, context);
        }

        public override string ToString() => $"LeapSecondCache@0x{_objAddress:X}";
    }

}
