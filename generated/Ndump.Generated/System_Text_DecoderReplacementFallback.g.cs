#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class DecoderReplacementFallback : _.System.Text.DecoderFallback
{
    private DecoderReplacementFallback(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _strDefault => StringField();

    public static new DecoderReplacementFallback FromAddress(ulong address, DumpContext ctx)
        => new DecoderReplacementFallback(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<DecoderReplacementFallback> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.DecoderReplacementFallback"))
            yield return new DecoderReplacementFallback(addr, ctx);
    }

    public override string ToString() => $"DecoderReplacementFallback@0x{_objAddress:X}";
}
