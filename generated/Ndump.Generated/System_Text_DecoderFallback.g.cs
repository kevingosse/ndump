#nullable enable
using Ndump.Core;

namespace _.System.Text;

public class DecoderFallback : _.System.Object
{
    protected DecoderFallback(ulong address, DumpContext context) : base(address, context) { }

    public static new DecoderFallback FromAddress(ulong address, DumpContext context)
        => new DecoderFallback(address, context);

    public static new global::System.Collections.Generic.IEnumerable<DecoderFallback> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.DecoderFallback"))
            yield return new DecoderFallback(addr, context);
    }

    public override string ToString() => $"DecoderFallback@0x{_objAddress:X}";
}
