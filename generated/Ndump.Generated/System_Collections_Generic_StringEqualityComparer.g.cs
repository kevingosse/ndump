#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class StringEqualityComparer : _.System.Collections.Generic.EqualityComparer_System_String_
{
    private StringEqualityComparer(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new StringEqualityComparer FromAddress(ulong address, DumpContext ctx)
        => new StringEqualityComparer(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StringEqualityComparer> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.StringEqualityComparer"))
            yield return new StringEqualityComparer(addr, ctx);
    }

    public override string ToString() => $"StringEqualityComparer@0x{_objAddress:X}";
}
