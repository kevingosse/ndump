#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class Module : _.System.Object
{
    protected Module(ulong address, DumpContext ctx) : base(address, ctx) { }

    public static new Module FromAddress(ulong address, DumpContext ctx)
        => new Module(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Module> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Reflection.Module"))
            yield return new Module(addr, ctx);
    }

    public override string ToString() => $"Module@0x{_objAddress:X}";
}
