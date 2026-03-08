#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Animal : _.System.Object
{
    protected Animal(ulong address, DumpContext context) : base(address, context) { }

    public string? _name => Field<string>();

    public int _age => Field<int>();

    public static new Animal FromAddress(ulong address, DumpContext context)
        => new Animal(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Animal> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Animal"))
            yield return new Animal(addr, context);
    }

    public override string ToString() => $"Animal@0x{_objAddress:X}";
}
