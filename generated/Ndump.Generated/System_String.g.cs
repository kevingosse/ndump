#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class String : global::_.System.Object
{
    private String(ulong address, DumpContext ctx) : base(address, ctx) { }

    public string? Value => _ctx.GetStringValue(_objAddress);

    public static implicit operator string?(String? proxy) => proxy?._ctx.GetStringValue(proxy._objAddress);

    public override string ToString() => _ctx.GetStringValue(_objAddress) ?? $"String@0x{_objAddress:X}";

    public static new String FromAddress(ulong address, DumpContext ctx)
        => new String(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<String> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.String"))
            yield return new String(addr, ctx);
    }
}
