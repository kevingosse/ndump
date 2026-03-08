#nullable enable
using Ndump.Core;

namespace _.System.Collections.Generic;

public sealed class StringEqualityComparer : global::_.System.Object
{
    private StringEqualityComparer(ulong address, DumpContext context) : base(address, context) { }

    public static new StringEqualityComparer FromAddress(ulong address, DumpContext context)
        => new StringEqualityComparer(address, context);

    public static new global::System.Collections.Generic.IEnumerable<StringEqualityComparer> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Collections.Generic.StringEqualityComparer"))
            yield return new StringEqualityComparer(addr, context);
    }

    public override string ToString() => $"StringEqualityComparer@0x{_objAddress:X}";
}
