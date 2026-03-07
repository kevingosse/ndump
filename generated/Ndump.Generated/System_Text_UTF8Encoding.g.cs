#nullable enable
using Ndump.Core;

namespace _.System.Text;

public partial class UTF8Encoding : _.System.Text.Encoding
{
    protected UTF8Encoding(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _emitUTF8Identifier => Field<bool>();

    public bool _isThrowException => Field<bool>();

    public static new UTF8Encoding FromAddress(ulong address, DumpContext ctx)
        => new UTF8Encoding(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<UTF8Encoding> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Text.UTF8Encoding"))
            yield return new UTF8Encoding(addr, ctx);
    }

    public override string ToString() => $"UTF8Encoding@0x{_objAddress:X}";
}
