#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class EncoderReplacementFallback : _.System.Text.EncoderFallback
{
    private EncoderReplacementFallback(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _strDefault => Field<string>();

    public static new EncoderReplacementFallback FromAddress(ulong address, DumpContext ctx)
        => new EncoderReplacementFallback(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EncoderReplacementFallback> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.EncoderReplacementFallback"))
            yield return new EncoderReplacementFallback(addr, ctx);
    }

    public override string ToString() => $"EncoderReplacementFallback@0x{_objAddress:X}";
}
