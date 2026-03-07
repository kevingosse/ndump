#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class OSEncoding : _.System.Text.Encoding
{
    private OSEncoding(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _encodingName => _ctx.GetStringField(_objAddress, "_encodingName");

    public static new OSEncoding FromAddress(ulong address, DumpContext ctx)
        => new OSEncoding(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<OSEncoding> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.OSEncoding"))
            yield return new OSEncoding(addr, ctx);
    }

    public override string ToString() => $"OSEncoding@0x{_objAddress:X}";
}
