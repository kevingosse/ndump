#nullable enable
using Ndump.Core;

namespace _.System;

public class Delegate : _.System.Object
{
    protected Delegate(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.Object? _target
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_target");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public _.System.Object? _methodBase
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_methodBase");
            return addr == 0 ? null : global::_.ProxyResolver.Resolve(addr, _ctx) as _.System.Object ?? _.System.Object.FromAddress(addr, _ctx);
        }
    }

    public nint _methodPtr => _ctx.GetFieldValue<nint>(_objAddress, "_methodPtr");

    public nint _methodPtrAux => _ctx.GetFieldValue<nint>(_objAddress, "_methodPtrAux");

    public static new Delegate FromAddress(ulong address, DumpContext ctx)
        => new Delegate(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<Delegate> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Delegate"))
            yield return new Delegate(addr, ctx);
    }

    public override string ToString() => $"Delegate@0x{_objAddress:X}";
}
