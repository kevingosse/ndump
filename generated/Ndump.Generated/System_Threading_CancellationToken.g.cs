#nullable enable
using Ndump.Core;

namespace _.System.Threading;

public sealed class CancellationToken : global::_.System.Object, global::Ndump.Core.IProxy<CancellationToken>
{
    private CancellationToken(ulong address, DumpContext context) : base(address, context) { }
    private CancellationToken(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public global::_.System.Object? _source => Field<global::_.System.Object>();

    public static new CancellationToken FromAddress(ulong address, DumpContext context)
        => new CancellationToken(address, context);

    public static CancellationToken FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new CancellationToken(address, context, interiorTypeName);

    public override string ToString() => $"CancellationToken@0x{_objAddress:X}";
}
