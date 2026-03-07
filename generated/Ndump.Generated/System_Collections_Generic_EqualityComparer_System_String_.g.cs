#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public class EqualityComparer_System_String_ : _.System.Object
{
    protected EqualityComparer_System_String_(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new EqualityComparer_System_String_ FromAddress(ulong address, DumpContext ctx)
        => new EqualityComparer_System_String_(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<EqualityComparer_System_String_> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Collections.Generic.EqualityComparer<System.String>"))
            yield return new EqualityComparer_System_String_(addr, ctx);
    }

    public override string ToString() => $"EqualityComparer_System_String_@0x{_objAddress:X}";
}
