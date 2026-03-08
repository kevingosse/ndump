#nullable enable
using Ndump.Core;
using System.Runtime.CompilerServices;

namespace _.System;

public class Object
{
    protected readonly ulong _objAddress;
    protected readonly DumpContext _ctx;
    // For interior struct fields: the CLR type name of this struct
    protected readonly string? _interiorTypeName;

    protected Object(ulong address, DumpContext ctx)
    {
        _objAddress = address;
        _ctx = ctx;
    }

    protected Object(ulong address, DumpContext ctx, string interiorTypeName)
    {
        _objAddress = address;
        _ctx = ctx;
        _interiorTypeName = interiorTypeName;
    }

    public ulong GetObjAddress() => _objAddress;

    protected T Field<T>([CallerMemberName] string fieldName = "")
    {
        if (_interiorTypeName is not null)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)_ctx.GetStringField(_objAddress, _interiorTypeName, fieldName)!;
            if (!typeof(T).IsValueType)
            {
                var addr = _ctx.GetObjectAddress(_objAddress, _interiorTypeName, fieldName);
                if (addr == 0) return default!;
                return ResolveProxy<T>(addr, _ctx);
            }
            return _ctx.GetFieldValue<T>(_objAddress, _interiorTypeName, fieldName);
        }
        if (typeof(T) == typeof(string))
            return (T)(object)_ctx.GetStringField(_objAddress, fieldName)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _ctx.GetObjectAddress(_objAddress, fieldName);
            if (addr == 0) return default!;
            return ResolveProxy<T>(addr, _ctx);
        }
        return _ctx.GetFieldValue<T>(_objAddress, fieldName);
    }

    protected T StructField<T>(string structTypeName, [CallerMemberName] string fieldName = "")
    {
        ulong addr;
        if (_interiorTypeName is not null)
            addr = _ctx.GetInteriorValueTypeFieldAddress(_objAddress, _interiorTypeName, fieldName);
        else
            addr = _ctx.GetValueTypeFieldAddress(_objAddress, fieldName);
        return (T)CreateInteriorProxy(typeof(T), addr, _ctx, structTypeName);
    }

    private static object CreateInteriorProxy(global::System.Type proxyType, ulong address, DumpContext ctx, string structTypeName)
    {
        var method = proxyType.GetMethod("FromInterior", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
        return method?.Invoke(null, [address, ctx, structTypeName]) ?? throw new global::System.InvalidOperationException($"No FromInterior factory on {proxyType}");
    }

    protected T? NullableField<T>([CallerMemberName] string fieldName = "") where T : struct
    {
        if (_interiorTypeName is not null)
            return _ctx.GetNullableFieldValue<T>(_objAddress, _interiorTypeName, fieldName);
        return _ctx.GetNullableFieldValue<T>(_objAddress, fieldName);
    }

    protected T? NullableStructField<T>(string innerTypeName, [CallerMemberName] string fieldName = "") where T : class
    {
        (bool hasValue, ulong valueAddr) info;
        if (_interiorTypeName is not null)
            info = _ctx.GetNullableFieldInfo(_objAddress, _interiorTypeName, fieldName);
        else
            info = _ctx.GetNullableFieldInfo(_objAddress, fieldName);
        if (!info.hasValue) return null;
        return (T)CreateInteriorProxy(typeof(T), info.valueAddr, _ctx, innerTypeName);
    }

    protected ulong RawFieldAddress([CallerMemberName] string fieldName = "")
    {
        if (_interiorTypeName is not null)
            return _ctx.GetInteriorValueTypeFieldAddress(_objAddress, _interiorTypeName, fieldName);
        return _ctx.GetValueTypeFieldAddress(_objAddress, fieldName);
    }

    protected ulong RefAddress([CallerMemberName] string fieldName = "")
        => _interiorTypeName is not null
            ? _ctx.GetObjectAddress(_objAddress, _interiorTypeName, fieldName)
            : _ctx.GetObjectAddress(_objAddress, fieldName);

    protected global::Ndump.Core.DumpArray<T>? ArrayField<T>([CallerMemberName] string fieldName = "")
    {
        var addr = RefAddress(fieldName);
        if (addr == 0) return null;
        var len = _ctx.GetArrayLength(addr);
        return new global::Ndump.Core.DumpArray<T>(addr, len, i => ReadArrayElement<T>(addr, i));
    }

    protected global::Ndump.Core.DumpArray<ulong>? ArrayAddresses([CallerMemberName] string fieldName = "")
    {
        var addr = RefAddress(fieldName);
        if (addr == 0) return null;
        var len = _ctx.GetArrayLength(addr);
        return new global::Ndump.Core.DumpArray<ulong>(addr, len, i => _ctx.GetArrayElementAddress(addr, i));
    }

    protected T ReadArrayElement<T>(ulong arrayAddr, int index)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)_ctx.GetArrayElementString(arrayAddr, index)!;
        if (!typeof(T).IsValueType)
        {
            var interiorMethod = typeof(T).GetMethod("FromInterior", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
            if (interiorMethod is not null)
            {
                var ea = _ctx.GetArrayStructElementAddress(arrayAddr, index);
                var typeName = _ctx.GetArrayComponentTypeName(arrayAddr);
                return (T)interiorMethod.Invoke(null, [ea, _ctx, typeName])!;
            }
            var addr = _ctx.GetArrayElementAddress(arrayAddr, index);
            if (addr == 0) return default!;
            return ResolveProxy<T>(addr, _ctx);
        }
        return _ctx.GetArrayElementValue<T>(arrayAddr, index);
    }

    private static T ResolveProxy<T>(ulong address, DumpContext ctx)
    {
        var resolved = global::_.ProxyResolver.Resolve(address, ctx);
        if (resolved is T t) return t;
        var method = typeof(T).GetMethod("FromAddress", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
        return (T)(method?.Invoke(null, [address, ctx]) ?? throw new global::System.InvalidOperationException($"No FromAddress factory on {typeof(T)}"));
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
