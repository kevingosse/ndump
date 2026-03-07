#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Tag : _.System.Object
{
    private Tag(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _label => StringField();

    public long _id => Field<long>();

    public static new Tag FromAddress(ulong address, DumpContext ctx)
        => new Tag(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Tag> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Tag"))
            yield return new Tag(addr, ctx);
    }

    public override string ToString() => $"Tag@0x{_objAddress:X}";
}
