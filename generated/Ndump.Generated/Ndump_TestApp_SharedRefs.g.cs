#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class SharedRefs : _.System.Object
{
    private SharedRefs(ulong address, DumpContext context) : base(address, context) { }

    public _.Ndump.TestApp.Tag? _ref1 => Field<_.Ndump.TestApp.Tag>();

    public _.Ndump.TestApp.Tag? _ref2 => Field<_.Ndump.TestApp.Tag>();

    public _.Ndump.TestApp.Address? _shared => Field<_.Ndump.TestApp.Address>();

    public _.Ndump.TestApp.Address? _sharedAgain => Field<_.Ndump.TestApp.Address>();

    public static new SharedRefs FromAddress(ulong address, DumpContext context)
        => new SharedRefs(address, context);

    public static new global::System.Collections.Generic.IEnumerable<SharedRefs> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.SharedRefs"))
            yield return new SharedRefs(addr, context);
    }

    public override string ToString() => $"SharedRefs@0x{_objAddress:X}";
}
