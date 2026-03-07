#nullable enable
using Ndump.Core;

namespace _.System.IO;

public class TextReader : _.System.MarshalByRefObject
{
    protected TextReader(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new TextReader FromAddress(ulong address, DumpContext ctx)
        => new TextReader(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<TextReader> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.IO.TextReader"))
            yield return new TextReader(addr, ctx);
    }

    public override string ToString() => $"TextReader@0x{_objAddress:X}";
}
