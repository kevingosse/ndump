#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Label : global::_.System.Object, global::Ndump.Core.IProxy<Label>
{
    private Label(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Label(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public string? Text => Field<string>();

    public int Priority => Field<int>();

    public _.System.Object? Metadata => Field<_.System.Object>();

    public static new Label FromAddress(ulong address, DumpContext ctx)
        => new Label(address, ctx);

    public static Label FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Label(address, ctx, interiorTypeName);

    public override string ToString() => $"Label@0x{_objAddress:X}";
}
