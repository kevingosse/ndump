#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Point : global::_.System.Object, global::Ndump.Core.IProxy<Point>
{
    private Point(ulong address, DumpContext context) : base(address, context) { }
    private Point(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public int X => Field<int>();

    public int Y => Field<int>();

    public static new Point FromAddress(ulong address, DumpContext context)
        => new Point(address, context);

    public static Point FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new Point(address, context, interiorTypeName);

    public override string ToString() => $"Point@0x{_objAddress:X}";
}
