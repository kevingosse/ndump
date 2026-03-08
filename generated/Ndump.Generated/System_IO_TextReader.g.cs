#nullable enable
using Ndump.Core;

namespace _.System.IO;

public class TextReader : _.System.MarshalByRefObject
{
    protected TextReader(ulong address, DumpContext context) : base(address, context) { }

    public static new TextReader FromAddress(ulong address, DumpContext context)
        => new TextReader(address, context);

    public static new global::System.Collections.Generic.IEnumerable<TextReader> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.IO.TextReader"))
            yield return new TextReader(addr, context);
    }

    public override string ToString() => $"TextReader@0x{_objAddress:X}";
}
