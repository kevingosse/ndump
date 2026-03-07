#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Address : _.System.Object
{
    private Address(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _street => _ctx.GetStringField(_objAddress, "_street");

    public string? _city => _ctx.GetStringField(_objAddress, "_city");

    public int _zipCode => _ctx.GetFieldValue<int>(_objAddress, "_zipCode");

    public static new Address FromAddress(ulong address, DumpContext ctx)
        => new Address(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Address> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Address"))
            yield return new Address(addr, ctx);
    }

    public override string ToString() => $"Address@0x{_objAddress:X}";
}
