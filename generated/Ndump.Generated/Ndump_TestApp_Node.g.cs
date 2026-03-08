#nullable enable
using Ndump.Core;

namespace _.Ndump.TestApp;

public sealed class Node : _.System.Object
{
    private Node(ulong address, DumpContext context) : base(address, context) { }

    public string? _name => Field<string>();

    public _.Ndump.TestApp.Node? _next => Field<_.Ndump.TestApp.Node>();

    public _.Ndump.TestApp.Node? _self => Field<_.Ndump.TestApp.Node>();

    public static new Node FromAddress(ulong address, DumpContext context)
        => new Node(address, context);

    public static new global::System.Collections.Generic.IEnumerable<Node> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("Ndump.TestApp.Node"))
            yield return new Node(addr, context);
    }

    public override string ToString() => $"Node@0x{_objAddress:X}";
}
