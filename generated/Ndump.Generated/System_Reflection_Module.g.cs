#nullable enable
using Ndump.Core;

namespace _.System.Reflection;

public class Module : _.System.Object
{
    protected Module(ulong address, DumpContext context) : base(address, context) { }

    public static new Module FromAddress(ulong address, DumpContext context)
        => new Module(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Module> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Reflection.Module"))
            yield return new Module(addr, context);
    }

    public override string ToString() => $"Module@0x{_objAddress:X}";
}
