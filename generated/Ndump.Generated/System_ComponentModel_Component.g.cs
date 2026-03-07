#nullable enable
using Ndump.Core;

namespace _.System.ComponentModel;

public class Component : _.System.MarshalByRefObject
{
    protected Component(ulong address, DumpContext ctx) : base(address, ctx) { }

    public ulong _site => _ctx.GetObjectAddress(_objAddress, "_site");

    public ulong _events => _ctx.GetObjectAddress(_objAddress, "_events");

    public static new Component FromAddress(ulong address, DumpContext ctx)
        => new Component(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Component> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.ComponentModel.Component"))
            yield return new Component(addr, ctx);
    }

    public override string ToString() => $"Component@0x{_objAddress:X}";
}
