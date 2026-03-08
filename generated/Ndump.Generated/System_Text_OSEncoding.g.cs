#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class OSEncoding : _.System.Text.Encoding
{
    private OSEncoding(ulong address, DumpContext context) : base(address, context) { }

    public string? _encodingName => Field<string>();

    public static new OSEncoding FromAddress(ulong address, DumpContext context)
        => new OSEncoding(address, context);

    public static new global::System.Collections.Generic.IEnumerable<OSEncoding> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.OSEncoding"))
            yield return new OSEncoding(addr, context);
    }

    public override string ToString() => $"OSEncoding@0x{_objAddress:X}";
}
