#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Point : global::_.System.Object
{
    private Point(ulong address, DumpContext ctx) : base(address, ctx) { }
    private Point(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public int X => Field<int>();

    public int Y => Field<int>();

    public static new Point FromAddress(ulong address, DumpContext ctx)
        => new Point(address, ctx);

    public static Point FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new Point(address, ctx, interiorTypeName);

    public override string ToString() => $"Point@0x{_objAddress:X}";
}
