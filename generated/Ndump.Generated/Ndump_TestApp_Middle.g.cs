#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public class Middle : _.Ndump.TestApp.Base
{
    protected Middle(ulong address, DumpContext context) : base(address, context) { }

    public string? _middleField => Field<string>();

    public static new Middle FromAddress(ulong address, DumpContext context)
        => new Middle(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Middle> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Middle"))
            yield return new Middle(addr, context);
    }

    public override string ToString() => $"Middle@0x{_objAddress:X}";
}
