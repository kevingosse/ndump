#nullable enable
using Ndump.Core;

namespace _.System;

public sealed class RuntimeType_RuntimeTypeCache : _.System.Object
{
    private RuntimeType_RuntimeTypeCache(ulong address, DumpContext ctx) : base(address, ctx) { }

    public _.System.RuntimeType? m_runtimeType
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_runtimeType");
            return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
        }
    }

    public _.System.RuntimeType? m_enclosingType
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "m_enclosingType");
            return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
        }
    }

    public int m_typeCode => _ctx.GetFieldValue<int>(_objAddress, "m_typeCode");

    public string? m_name => _ctx.GetStringField(_objAddress, "m_name");

    public string? m_fullName => _ctx.GetStringField(_objAddress, "m_fullName");

    public string? m_assemblyQualifiedName => _ctx.GetStringField(_objAddress, "m_assemblyQualifiedName");

    public string? m_toString => _ctx.GetStringField(_objAddress, "m_toString");

    public string? m_namespace => _ctx.GetStringField(_objAddress, "m_namespace");

    public bool m_isGlobal => _ctx.GetFieldValue<bool>(_objAddress, "m_isGlobal");

    // Unknown field: m_methodInfoCache (object)

    // Unknown field: m_constructorInfoCache (object)

    // Unknown field: m_fieldInfoCache (object)

    // Unknown field: m_interfaceCache (object)

    // Unknown field: m_nestedClassesCache (object)

    // Unknown field: m_propertyInfoCache (object)

    // Unknown field: m_eventInfoCache (object)

    public string? m_defaultMemberName => _ctx.GetStringField(_objAddress, "m_defaultMemberName");

    public ulong m_genericCache => _ctx.GetObjectAddress(_objAddress, "m_genericCache");

    public global::Ndump.Core.DumpArray<_.System.Object?>? _emptyArray
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_emptyArray");
            if (addr == 0) return null;
            var len = _ctx.GetArrayLength(addr);
            return new global::Ndump.Core.DumpArray<_.System.Object?>(addr, len, i => { var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : global::_.ProxyResolver.Resolve(ea, _ctx) as _.System.Object ?? _.System.Object.FromAddress(ea, _ctx); });
        }
    }

    public _.System.RuntimeType? _genericTypeDefinition
    {
        get
        {
            var addr = _ctx.GetObjectAddress(_objAddress, "_genericTypeDefinition");
            return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
        }
    }

    public static new RuntimeType_RuntimeTypeCache FromAddress(ulong address, DumpContext ctx)
        => new RuntimeType_RuntimeTypeCache(address, ctx);

    public static new global::System.Collections.Generic.IEnumerable<RuntimeType_RuntimeTypeCache> GetInstances(DumpContext ctx)
    {
        foreach (var addr in ctx.EnumerateInstances("System.RuntimeType+RuntimeTypeCache"))
            yield return new RuntimeType_RuntimeTypeCache(addr, ctx);
    }

    public override string ToString() => $"RuntimeType_RuntimeTypeCache@0x{_objAddress:X}";
}
