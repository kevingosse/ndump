#nullable enable
using Ndump.Core;

namespace _.System;

public partial class RuntimeType
{
    public sealed class RuntimeTypeCache : _.System.Object
    {
        private RuntimeTypeCache(ulong address, DumpContext ctx) : base(address, ctx) { }

        public _.System.RuntimeType? m_runtimeType => Field<_.System.RuntimeType>();

        public _.System.RuntimeType? m_enclosingType => Field<_.System.RuntimeType>();

        public int m_typeCode => Field<int>();

        public string? m_name => Field<string>();

        public string? m_fullName => Field<string>();

        public string? m_assemblyQualifiedName => Field<string>();

        public string? m_toString => Field<string>();

        public string? m_namespace => Field<string>();

        public bool m_isGlobal => Field<bool>();

        // Unknown field: m_methodInfoCache (object)

        // Unknown field: m_constructorInfoCache (object)

        // Unknown field: m_fieldInfoCache (object)

        // Unknown field: m_interfaceCache (object)

        // Unknown field: m_nestedClassesCache (object)

        // Unknown field: m_propertyInfoCache (object)

        // Unknown field: m_eventInfoCache (object)

        public string? m_defaultMemberName => Field<string>();

        public ulong m_genericCache => RefAddress();

        public global::Ndump.Core.DumpArray<_.System.Object?>? _emptyArray => ArrayField<_.System.Object?>();

        public _.System.RuntimeType? _genericTypeDefinition => Field<_.System.RuntimeType>();

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
