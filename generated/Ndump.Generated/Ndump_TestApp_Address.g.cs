#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Address : _.System.Object
{
    private Address(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _street => Field<string>();

    public string? _city => Field<string>();

    public int _zipCode => Field<int>();

    public static new Address FromAddress(ulong address, DumpContext ctx)
        => new Address(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Address> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Address"))
            yield return new Address(addr, ctx);
    }

    public override string ToString() => $"Address@0x{_objAddress:X}";
}
