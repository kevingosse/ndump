#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class CachedCompletedInt32Task : global::_.System.Object
{
    private CachedCompletedInt32Task(ulong address, DumpContext ctx) : base(address, ctx) { }
    private CachedCompletedInt32Task(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) { }

    public _.System.Threading.Tasks.Task<object>? _task => Field<_.System.Threading.Tasks.Task<object>>();

    public static new CachedCompletedInt32Task FromAddress(ulong address, DumpContext ctx)
        => new CachedCompletedInt32Task(address, ctx);

    public static CachedCompletedInt32Task FromInterior(ulong address, DumpContext ctx, string interiorTypeName)
        => new CachedCompletedInt32Task(address, ctx, interiorTypeName);

    public override string ToString() => $"CachedCompletedInt32Task@0x{_objAddress:X}";
}
