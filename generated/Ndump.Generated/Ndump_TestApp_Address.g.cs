#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Address : _.System.Object
{
    private Address(ulong address, DumpContext context) : base(address, context) { }

    public string? _street => Field<string>();

    public string? _city => Field<string>();

    public int _zipCode => Field<int>();

    public static new Address FromAddress(ulong address, DumpContext context)
        => new Address(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Address> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Address"))
            yield return new Address(addr, context);
    }

    public override string ToString() => $"Address@0x{_objAddress:X}";
}
