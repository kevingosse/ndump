#nullable enable
using Ndump.Core;

namespace _.System;

public partial class RuntimeType
{
    public sealed class RuntimeTypeCache : _.System.Object
    {
        private RuntimeTypeCache(ulong address, DumpContext ctx) : base(address, ctx) { }

        public _.System.RuntimeType? m_runtimeType
        {
            get
            {
                var addr = RefAddress();
                return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
            }
        }

        public _.System.RuntimeType? m_enclosingType
        {
            get
            {
                var addr = RefAddress();
                return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
            }
        }

        public int m_typeCode => Field<int>();

        public string? m_name => StringField();

        public string? m_fullName => StringField();

        public string? m_assemblyQualifiedName => StringField();

        public string? m_toString => StringField();

        public string? m_namespace => StringField();

        public bool m_isGlobal => Field<bool>();

        // Unknown field: m_methodInfoCache (object)

        // Unknown field: m_constructorInfoCache (object)

        // Unknown field: m_fieldInfoCache (object)

        // Unknown field: m_interfaceCache (object)

        // Unknown field: m_nestedClassesCache (object)

        // Unknown field: m_propertyInfoCache (object)

        // Unknown field: m_eventInfoCache (object)

        public string? m_defaultMemberName => StringField();

        public ulong m_genericCache => RefAddress();

        public global::Ndump.Core.DumpArray<_.System.Object?>? _emptyArray
        {
            get
            {
                var addr = RefAddress();
                if (addr == 0) return null;
                var len = _ctx.GetArrayLength(addr);
                return new global::Ndump.Core.DumpArray<_.System.Object?>(addr, len, i => { var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : global::_.ProxyResolver.Resolve(ea, _ctx) as _.System.Object ?? _.System.Object.FromAddress(ea, _ctx); });
            }
        }

        public _.System.RuntimeType? _genericTypeDefinition
        {
            get
            {
                var addr = RefAddress();
                return addr == 0 ? null : _.System.RuntimeType.FromAddress(addr, _ctx);
            }
        }

        public static new RuntimeTypeCache FromAddress(ulong address, DumpContext ctx)
            => new RuntimeTypeCache(address, ctx);

        public static new global::System.Collections.Generic.IEnumerable<RuntimeTypeCache> GetInstances(DumpContext ctx)
        {
            foreach (var addr in ctx.EnumerateInstances("System.RuntimeType+RuntimeTypeCache"))
                yield return new RuntimeTypeCache(addr, ctx);
        }

        public override string ToString() => $"RuntimeTypeCache@0x{_objAddress:X}";
    }

}
