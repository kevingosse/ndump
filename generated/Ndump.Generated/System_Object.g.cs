#nullable enable
using Ndump.Core;
using System.Runtime.CompilerServices;

namespace _.System;

public class Object
{
    protected readonly ulong _objAddress;
    protected readonly DumpContext _context;
    // For interior struct fields: the CLR type name of this struct
    protected readonly string? _interiorTypeName;

    protected Object(ulong address, DumpContext context)
    {
        _objAddress = address;
        _context = context;
    }

    protected Object(ulong address, DumpContext context, string interiorTypeName)
    {
        _objAddress = address;
        _context = context;
        _interiorTypeName = interiorTypeName;
    }

    public ulong GetObjectAddress() => _objAddress;

    protected T Field<T>([CallerMemberName] string fieldName = "")
    {
        if (typeof(T) == typeof(string))
            return (T)(object)_context.GetStringField(_objAddress, fieldName, _interiorTypeName)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _context.GetObjectAddress(_objAddress, fieldName, _interiorTypeName);
            if (addr == 0) return default!;
            return global::_.ProxyResolver.Resolve<T>(addr, _context);
        }
        return _context.GetFieldValue<T>(_objAddress, fieldName, _interiorTypeName);
    }

    protected T StructField<T>(string structTypeName, [CallerMemberName] string fieldName = "") where T : global::Ndump.Core.IProxy<T>
        => T.FromInterior(_context.GetValueTypeFieldAddress(_objAddress, fieldName, _interiorTypeName), _context, structTypeName);

    protected T? NullableField<T>([CallerMemberName] string fieldName = "") where T : struct
        => _context.GetNullableFieldValue<T>(_objAddress, fieldName, _interiorTypeName);

    protected T? NullableStructField<T>(string innerTypeName, [CallerMemberName] string fieldName = "") where T : class, global::Ndump.Core.IProxy<T>
    {
        var info = _context.GetNullableFieldInfo(_objAddress, fieldName, _interiorTypeName);
        if (!info.HasValue) return null;
        return T.FromInterior(info.ValueAddress, _context, innerTypeName);
    }

    protected ulong RawFieldAddress([CallerMemberName] string fieldName = "")
        => _context.GetValueTypeFieldAddress(_objAddress, fieldName, _interiorTypeName);

    protected ulong RefAddress([CallerMemberName] string fieldName = "")
        => _context.GetObjectAddress(_objAddress, fieldName, _interiorTypeName);

    protected global::Ndump.Core.DumpArray<T>? ArrayField<T>([CallerMemberName] string fieldName = "")
    {
        var addr = RefAddress(fieldName);
        if (addr == 0) return null;
        var len = _context.GetArrayLength(addr);
        return new global::Ndump.Core.DumpArray<T>(addr, len, i => ReadArrayElement<T>(addr, i));
    }

    protected global::Ndump.Core.DumpArray<ulong>? ArrayAddresses([CallerMemberName] string fieldName = "")
    {
        var addr = RefAddress(fieldName);
        if (addr == 0) return null;
        var len = _context.GetArrayLength(addr);
        return new global::Ndump.Core.DumpArray<ulong>(addr, len, i => _context.GetArrayElementAddress(addr, i));
    }

    protected T ReadArrayElement<T>(ulong arrayAddr, int index)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)_context.GetArrayElementString(arrayAddr, index)!;
        if (!typeof(T).IsValueType)
        {
            var addr = _context.GetArrayElementAddress(arrayAddr, index);
            if (addr == 0) return default!;
            return global::_.ProxyResolver.Resolve<T>(addr, _context);
        }
        return _context.GetArrayElementValue<T>(arrayAddr, index);
    }

    protected global::Ndump.Core.DumpArray<T>? StructArrayField<T>([CallerMemberName] string fieldName = "") where T : global::Ndump.Core.IProxy<T>
    {
        var addr = RefAddress(fieldName);
        if (addr == 0) return null;
        var len = _context.GetArrayLength(addr);
        var typeName = _context.GetArrayComponentTypeName(addr);
        return new global::Ndump.Core.DumpArray<T>(addr, len, i =>
        {
            var ea = _context.GetArrayStructElementAddress(addr, i);
            return T.FromInterior(ea, _context, typeName);
        });
    }
    public static Object FromAddress(ulong address, DumpContext context)
        => new Object(address, context);

    public static global::System.Collections.Generic.IEnumerable<Object> GetInstances(DumpContext context)
    {
        foreach (var addr in context.EnumerateInstances("System.Object"))
            yield return new Object(addr, context);
    }

    public override string ToString() => $"Object@0x{_objAddress:X}";
}
