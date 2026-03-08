#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class UTF8Encoding
{
    public sealed class UTF8EncodingSealed : _.System.Text.UTF8Encoding
    {
        private UTF8EncodingSealed(ulong address, DumpContext context) : base(address, context) { }

        public static new UTF8EncodingSealed FromAddress(ulong address, DumpContext context)
            => new UTF8EncodingSealed(address, context);

        public static new global::System.Collections.Generic.IEnumerable<UTF8EncodingSealed> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Text.UTF8Encoding+UTF8EncodingSealed"))
                yield return new UTF8EncodingSealed(addr, context);
        }

        public override string ToString() => $"UTF8EncodingSealed@0x{_objAddress:X}";
    }

}
