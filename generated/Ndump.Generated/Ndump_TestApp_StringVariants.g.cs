#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class StringVariants : _.System.Object
{
    private StringVariants(ulong address, DumpContext context) : base(address, context) { }

    public string? _normal => Field<string>();

    public string? _nullString => Field<string>();

    public string? _empty => Field<string>();

    public string? _unicode => Field<string>();

    public string? _long => Field<string>();

    public static new StringVariants FromAddress(ulong address, DumpContext context)
        => new StringVariants(address, context);

    public static new global::System.Collections.Generic.IEnumerable<StringVariants> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.StringVariants"))
            yield return new StringVariants(addr, context);
    }

    public override string ToString() => $"StringVariants@0x{_objAddress:X}";
}
