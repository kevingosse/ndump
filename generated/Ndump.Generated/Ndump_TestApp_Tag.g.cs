#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Tag : _.System.Object
{
    private Tag(ulong address, DumpContext context) : base(address, context) { }

    public string? _label => Field<string>();

    public long _id => Field<long>();

    public static new Tag FromAddress(ulong address, DumpContext context)
        => new Tag(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Tag> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Tag"))
            yield return new Tag(addr, context);
    }

    public override string ToString() => $"Tag@0x{_objAddress:X}";
}
