#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Label : global::_.System.Object, global::Ndump.Core.IProxy<Label>
{
    private Label(ulong address, DumpContext context) : base(address, context) { }
    private Label(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public string? Text => Field<string>();

    public int Priority => Field<int>();

    public _.System.Object? Metadata => Field<_.System.Object>();

    public static new Label FromAddress(ulong address, DumpContext context)
        => new Label(address, context);

    public static Label FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Label(address, context, interiorTypeName);

    public override string ToString() => $"Label@0x{_objAddress:X}";
}
