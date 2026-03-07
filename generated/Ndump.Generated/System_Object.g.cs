#nullable enable
using Ndump.Core;
using System.Runtime.CompilerServices;

namespace _.System;

public class Object
{
    protected readonly ulong _objAddress;
    protected readonly DumpContext _ctx;

    protected Object(ulong address, DumpContext ctx)
    {
        _objAddress = address;
        _ctx = ctx;
    }

    public ulong GetObjAddress() => _objAddress;

    protected T Field<T>([CallerMemberName] string fieldName = "") where T : unmanaged
        => _ctx.GetFieldValue<T>(_objAddress, fieldName);

    protected string? StringField([CallerMemberName] string fieldName = "")
        => _ctx.GetStringField(_objAddress, fieldName);

    protected ulong RefAddress([CallerMemberName] string fieldName = "")
        => _ctx.GetObjectAddress(_objAddress, fieldName);

    public static Object FromAddress(ulong address, DumpContext ctx)
        => new Object(address, ctx);

    public static global::System.Collections.Generic.IEnumerable<Object> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Object"))
            yield return new Object(addr, ctx);
    }

    public override string ToString() => $"Object@0x{_objAddress:X}";
}
