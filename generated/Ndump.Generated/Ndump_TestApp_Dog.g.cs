#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Dog : _.Ndump.TestApp.Animal
{
    private Dog(ulong address, DumpContext context) : base(address, context) { }

    public string? _breed => Field<string>();

    public static new Dog FromAddress(ulong address, DumpContext context)
        => new Dog(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Dog> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Dog"))
            yield return new Dog(addr, context);
    }

    public override string ToString() => $"Dog@0x{_objAddress:X}";
}
