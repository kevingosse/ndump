#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class VoidTaskResult : global::_.System.Object
{
    private VoidTaskResult(ulong address, DumpContext ctx) : base(address, ctx) { }
    private VoidTaskResult(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex) : base(address, ctx, arrayAddr, arrayIndex) { }
    private VoidTaskResult(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public static new VoidTaskResult FromAddress(ulong address, DumpContext ctx)
        => new VoidTaskResult(address, ctx);

    public static VoidTaskResult FromArrayElement(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
        => new VoidTaskResult(address, ctx, arrayAddr, arrayIndex);

    public static VoidTaskResult FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new VoidTaskResult(address, ctx, interiorTypeName);

    public override string ToString() => $"VoidTaskResult@0x{_objAddress:X}";
}
