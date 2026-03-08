#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Rectangle : global::_.System.Object, global::Ndump.Core.IProxy<Rectangle>
{
    private Rectangle(ulong address, DumpContext context) : base(address, context) { }
    private Rectangle(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public _.Ndump.TestApp.Point TopLeft => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public _.Ndump.TestApp.Point BottomRight => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public static new Rectangle FromAddress(ulong address, DumpContext context)
        => new Rectangle(address, context);

    public static Rectangle FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Rectangle(address, context, interiorTypeName);

    public override string ToString() => $"Rectangle@0x{_objAddress:X}";
}
