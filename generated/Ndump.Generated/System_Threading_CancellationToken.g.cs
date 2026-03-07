#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class CancellationToken : global::_.System.Object
{
    private CancellationToken(ulong address, DumpContext ctx) : base(address, ctx) { }
    private CancellationToken(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private CancellationToken(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public global::_.System.Object? _source => Field<global::_.System.Object>();

    public static new CancellationToken FromAddress(ulong address, DumpContext ctx)
        => new CancellationToken(address, ctx);

    public static CancellationToken FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new CancellationToken(address, ctx, arrayAddr, arrayIndex);

    public static CancellationToken FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new CancellationToken(address, ctx, interiorTypeName);

    public override string ToString() => $"CancellationToken@0x{_objAddress:X}";
}
