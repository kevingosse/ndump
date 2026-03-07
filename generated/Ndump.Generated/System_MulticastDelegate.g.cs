#nullable enable
using Ndump.Core;

namespace _.System;

public class MulticastDelegate : _.System.Delegate
{
    protected MulticastDelegate(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Object? _invocationList
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_invocationList");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public nint _invocationCount => _ctx.GetFieldValue<nint>(_objAddress, "_invocationCount");

    public static new MulticastDelegate FromAddress(ulong address, DumpContext ctx)
        => new MulticastDelegate(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<MulticastDelegate> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.MulticastDelegate"))
            yield return new MulticastDelegate(addr, ctx);
    }

    public override string ToString() => $"MulticastDelegate@0x{_objAddress:X}";
}
