#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Node : _.System.Object
{
    private Node(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? _name => Field<string>();

    public _.Ndump.TestApp.Node? _next => Field<_.Ndump.TestApp.Node>();

    public _.Ndump.TestApp.Node? _self => Field<_.Ndump.TestApp.Node>();

    public static new Node FromAddress(ulong address, DumpContext ctx)
        => new Node(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Node> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("Ndump.TestApp.Node"))
            yield return new Node(addr, ctx);
    }

    public override string ToString() => $"Node@0x{_objAddress:X}";
}
