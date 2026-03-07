#nullable enable
using Ndump.Core;

namespace _.System.Threading.Tasks;

public sealed class Task___c : _.System.Object
{
    private Task___c(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Task___c FromAddress(ulong address, DumpContext ctx)
        => new Task___c(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Task___c> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Threading.Tasks.Task+<>c"))
            yield return new Task___c(addr, ctx);
    }

    public override string ToString() => $"Task___c@0x{_objAddress:X}";
}
