#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class String : global::_.System.Object
{
    private String(ulong address, DumpContext context) : base(address, context) { }

    public string? Value => _context.GetStringValue(_objAddress);

    public static implicit operator string?(String? proxy) => proxy?._context.GetStringValue(proxy._objAddress);

    public override string ToString() => _context.GetStringValue(_objAddress) ?? $"String@0x{_objAddress:X}";

    public static new String FromAddress(ulong address, DumpContext context)
        => new String(address, context);

    public static new global::System.Collections.Generic.IEnumerable<String> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.String"))
            yield return new String(addr, context);
    }
}
