#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class ConsoleEncoding : _.System.Text.Encoding
{
    private ConsoleEncoding(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Text.Encoding? _encoding => Field<_.System.Text.Encoding>();

    public static new ConsoleEncoding FromAddress(ulong address, DumpContext ctx)
        => new ConsoleEncoding(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<ConsoleEncoding> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.ConsoleEncoding"))
            yield return new ConsoleEncoding(addr, ctx);
    }

    public override string ToString() => $"ConsoleEncoding@0x{_objAddress:X}";
}
