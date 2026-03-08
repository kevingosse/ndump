#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class EncoderFallback : _.System.Object
{
    protected EncoderFallback(ulong address, DumpContext context) : base(address, context) { }

    public static new EncoderFallback FromAddress(ulong address, DumpContext context)
        => new EncoderFallback(address, context);

    public static new global::System.Collections.Generic.IEnumerable<EncoderFallback> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.EncoderFallback"))
            yield return new EncoderFallback(addr, context);
    }

    public override string ToString() => $"EncoderFallback@0x{_objAddress:X}";
}
