#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Cat : _.Ndump.TestApp.Animal
{
    private Cat(ulong address, DumpContext ctx) : base(address, ctx) { }

    public bool _isIndoor => _ctx.GetFieldValue<bool>(_objAddress, "_isIndoor");

    public static new Cat FromAddress(ulong address, DumpContext ctx)
        => new Cat(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Cat> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Cat"))
            yield return new Cat(addr, ctx);
    }

    public override string ToString() => $"Cat@0x{_objAddress:X}";
}
