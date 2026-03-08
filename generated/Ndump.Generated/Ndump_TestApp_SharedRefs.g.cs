#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class SharedRefs : _.System.Object
{
    private SharedRefs(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.Ndump.TestApp.Tag? _ref1 => Field<_.Ndump.TestApp.Tag>();

    public _.Ndump.TestApp.Tag? _ref2 => Field<_.Ndump.TestApp.Tag>();

    public _.Ndump.TestApp.Address? _shared => Field<_.Ndump.TestApp.Address>();

    public _.Ndump.TestApp.Address? _sharedAgain => Field<_.Ndump.TestApp.Address>();

    public static new SharedRefs FromAddress(ulong address, DumpContext ctx)
        => new SharedRefs(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<SharedRefs> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.SharedRefs"))
            yield return new SharedRefs(addr, ctx);
    }

    public override string ToString() => $"SharedRefs@0x{_objAddress:X}";
}
