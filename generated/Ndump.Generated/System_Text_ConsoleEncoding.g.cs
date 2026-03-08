#nullable enable
using Ndump.Core;

namespace _.System.Text;

public sealed class ConsoleEncoding : _.System.Text.Encoding
{
    private ConsoleEncoding(ulong address, DumpContext context) : base(address, context) { }

    public _.System.Text.Encoding? _encoding => Field<_.System.Text.Encoding>();

    public static new ConsoleEncoding FromAddress(ulong address, DumpContext context)
        => new ConsoleEncoding(address, context);

    public static new global::System.Collections.Generic.IEnumerable<ConsoleEncoding> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Text.ConsoleEncoding"))
            yield return new ConsoleEncoding(addr, context);
    }

    public override string ToString() => $"ConsoleEncoding@0x{_objAddress:X}";
}
