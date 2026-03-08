#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class DecoderReplacementFallback : _.System.Text.DecoderFallback
{
    private DecoderReplacementFallback(ulong address, DumpContext context) : base(address, context) { }

    public string? _strDefault => Field<string>();

    public static new DecoderReplacementFallback FromAddress(ulong address, DumpContext context)
        => new DecoderReplacementFallback(address, context);

    public static new global::System.Collections.Generic.IEnumerable<DecoderReplacementFallback> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.DecoderReplacementFallback"))
            yield return new DecoderReplacementFallback(addr, context);
    }

    public override string ToString() => $"DecoderReplacementFallback@0x{_objAddress:X}";
}
