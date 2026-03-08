#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class CachedCompletedInt32Task : global::_.System.Object, global::Ndump.Core.IProxy<CachedCompletedInt32Task>
{
    private CachedCompletedInt32Task(ulong address, DumpContext context) : base(address, context) { }
    private CachedCompletedInt32Task(ulong address, DumpContext context, string interiorTypeName) : base(address, context, interiorTypeName) { }

    public _.System.Threading.Tasks.Task<object>? _task => Field<_.System.Threading.Tasks.Task<object>>();

    public static new CachedCompletedInt32Task FromAddress(ulong address, DumpContext context)
        => new CachedCompletedInt32Task(address, context);

    public static CachedCompletedInt32Task FromInterior(ulong address, DumpContext context, string interiorTypeName)
        => new CachedCompletedInt32Task(address, context, interiorTypeName);

    public override string ToString() => $"CachedCompletedInt32Task@0x{_objAddress:X}";
}
