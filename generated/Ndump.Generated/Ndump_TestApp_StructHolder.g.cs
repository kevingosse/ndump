#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class StructHolder : _.System.Object
{
    private StructHolder(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.Ndump.TestApp.Point _position => StructField<_.Ndump.TestApp.Point>("Ndump.TestApp.Point");

    public _.Ndump.TestApp.Rectangle _bounds => StructField<_.Ndump.TestApp.Rectangle>("Ndump.TestApp.Rectangle");

    public _.Ndump.TestApp.Label _label => StructField<_.Ndump.TestApp.Label>("Ndump.TestApp.Label");

    public static new StructHolder FromAddress(ulong address, DumpContext ctx)
        => new StructHolder(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<StructHolder> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.StructHolder"))
            yield return new StructHolder(addr, ctx);
    }

    public override string ToString() => $"StructHolder@0x{_objAddress:X}";
}
