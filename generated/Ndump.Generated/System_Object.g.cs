#nullable enable
using Ndump.Core;
using System.Runtime.CompilerServices;

namespace _.System;

public class Object
{
    protected readonly ulong _objAddress;
    protected readonly DumpContext _ctx;
    // For struct proxies: array address + index for correct component type resolution
    protected readonly ulong _arrayAddr;
    protected readonly int _arrayIndex;
    protected readonly bool _isStructElement;

    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<global::System.Type, global::System.Func<ulong, DumpContext, object>?> _proxyFactories = new();

    protected Object(ulong address, DumpContext ctx)
    {
        _objAddress = address;
        _ctx = ctx;
    }

    protected Object(ulong address, DumpContext ctx, ulong arrayAddr, int arrayIndex)
    {
        _objAddress = address;
        _ctx = ctx;
        _arrayAddr = arrayAddr;
        _arrayIndex = arrayIndex;
        _isStructElement = true;
    }

    public ulong GetObjAddress() => _objAddress;

    protected T Field<T>([CallerMemberName] string fieldName = "")
    {
        if (_isStructElement)
            return FieldStructElement<T>(fieldName);
        if (typeof(T) == typeof(string))
            return (T)(object)_ctx.GetStringField(_objAddress, fieldName)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _ctx.GetObjectAddress(_objAddress, fieldName);
            if (addr == 0) return default!;
            return (T)CreateProxy(typeof(T), addr, _ctx);
        }
        return _ctx.GetFieldValue<T>(_objAddress, fieldName);
    }

    private T FieldStructElement<T>(string fieldName)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)_ctx.GetStructArrayElementStringField(_arrayAddr, _arrayIndex, fieldName)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _ctx.GetStructArrayElementObjectAddress(_arrayAddr, _arrayIndex, fieldName);
            if (addr == 0) return default!;
            return (T)CreateProxy(typeof(T), addr, _ctx);
        }
        return _ctx.GetStructArrayElementFieldValue<T>(_arrayAddr, _arrayIndex, fieldName);
    }

    protected ulong RefAddress([CallerMemberName] string fieldName = "")
        => _isStructElement
            ? _ctx.GetStructArrayElementObjectAddress(_arrayAddr, _arrayIndex, fieldName)
            : _ctx.GetObjectAddress(_objAddress, fieldName);

    protected T ReadArrayElement<T>(ulong arrayAddr, int index)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)_ctx.GetArrayElementString(arrayAddr, index)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _ctx.GetArrayElementAddress(arrayAddr, index);
            if (addr == 0) return default!;
            return (T)CreateProxy(typeof(T), addr, _ctx);
        }
        return _ctx.GetArrayElementValue<T>(arrayAddr, index);
    }

    private static object CreateProxy(global::System.Type proxyType, ulong address, DumpContext ctx)
    {
        var factory = _proxyFactories.GetOrAdd(proxyType, static t =>
        {
            var method = t.GetMethod("FromAddress", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
            if (method is null) return null;
            return (global::System.Func<ulong, DumpContext, object>)((a, c) => method.Invoke(null, [a, c])!);
        });
        return factory?.Invoke(address, ctx) ?? throw new global::System.InvalidOperationException($"No FromAddress factory on {proxyType}");
    }

    public static Object FromAddress(ulong address, DumpContext ctx)
        => new Object(address, ctx);

    public static global::System.Collections.Generic.IEnumerable<Object> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.Object"))
            yield return new Object(addr, ctx);
    }

    public override string ToString() => $"Object@0x{_objAddress:X}";
}
