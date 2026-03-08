#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class Encoding
{
    public sealed class DefaultDecoder : _.System.Text.Decoder
    {
        private DefaultDecoder(ulong address, DumpContext context) : base(address, context) { }

        public _.System.Text.Encoding? _encoding => Field<_.System.Text.Encoding>();

        public static new DefaultDecoder FromAddress(ulong address, DumpContext context)
            => new DefaultDecoder(address, context);

        public static new global::System.Collections.Generic.IEnumerable<DefaultDecoder> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.Text.Encoding+DefaultDecoder"))
                yield return new DefaultDecoder(addr, context);
        }

        public override string ToString() => $"DefaultDecoder@0x{_objAddress:X}";
    }

}
