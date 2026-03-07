#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class UTF8Encoding_UTF8EncodingSealed : _.System.Text.UTF8Encoding
{
    private UTF8Encoding_UTF8EncodingSealed(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new UTF8Encoding_UTF8EncodingSealed FromAddress(ulong address, DumpContext ctx)
        => new UTF8Encoding_UTF8EncodingSealed(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<UTF8Encoding_UTF8EncodingSealed> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.UTF8Encoding+UTF8EncodingSealed"))
            yield return new UTF8Encoding_UTF8EncodingSealed(addr, ctx);
    }

    public override string ToString() => $"UTF8Encoding_UTF8EncodingSealed@0x{_objAddress:X}";
}
