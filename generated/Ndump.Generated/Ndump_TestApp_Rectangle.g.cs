#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Rectangle : global::_.System.Object, global::Ndump.Core.IProxy<Rectangle>
{
    private Rectangle(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Rectangle(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public _.Ndump.TestApp.Point TopLeft => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public _.Ndump.TestApp.Point BottomRight => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public static new Rectangle FromAddress(ulong address, DumpContext ctx)
        => new Rectangle(address, ctx);

    public static Rectangle FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Rectangle(address, ctx, interiorTypeName);

    public override string ToString() => $"Rectangle@0x{_objAddress:X}";
}
