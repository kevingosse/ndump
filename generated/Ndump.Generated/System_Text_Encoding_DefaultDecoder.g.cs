#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class Encoding
{
    public sealed class DefaultDecoder : _.System.Text.Decoder
    {
        private DefaultDecoder(ulong address, DumpContext ctx) : base(address, ctx) { }

        public _.System.Text.Encoding? _encoding
        {
            get
            {
                var addr = RefAddress();
                return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Text.Encoding ?? _.System.Text.Encoding.FromAddress(addr, _ctx);
            }
        }

        public static new DefaultDecoder FromAddress(ulong address, DumpContext ctx)
            => new DefaultDecoder(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<DefaultDecoder> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.Text.Encoding+DefaultDecoder"))
                yield return new DefaultDecoder(addr, ctx);
        }

        public override string ToString() => $"DefaultDecoder@0x{_objAddress:X}";
    }

}
