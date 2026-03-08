#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class VoidTaskResult : global::_.System.Object, global::Ndump.Core.IProxy<VoidTaskResult>
{
    private VoidTaskResult(ulong address, DumpContext context) : base(address, context) { }
    private VoidTaskResult(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public static new VoidTaskResult FromAddress(ulong address, DumpContext context)
        => new VoidTaskResult(address, context);

    public static VoidTaskResult FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new VoidTaskResult(address, context, interiorTypeName);

    public override string ToString() => $"VoidTaskResult@0x{_objAddress:X}";
}
