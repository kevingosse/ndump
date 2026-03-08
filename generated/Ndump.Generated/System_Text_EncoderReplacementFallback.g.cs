#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class EncoderReplacementFallback : _.System.Text.EncoderFallback
{
    private EncoderReplacementFallback(ulong address, DumpContext context) : base(address, context) { }

    public string? _strDefault => Field<string>();

    public static new EncoderReplacementFallback FromAddress(ulong address, DumpContext context)
        => new EncoderReplacementFallback(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EncoderReplacementFallback> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.EncoderReplacementFallback"))
            yield return new EncoderReplacementFallback(addr, context);
    }

    public override string ToString() => $"EncoderReplacementFallback@0x{_objAddress:X}";
}
