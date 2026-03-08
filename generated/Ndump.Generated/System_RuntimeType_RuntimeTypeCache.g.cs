#nullable enable
using Ndump.Core;

namespace _.System;

public partial class RuntimeType
{
    public sealed class RuntimeTypeCache : _.System.Object
    {
        private RuntimeTypeCache(ulong address, DumpContext context) : base(address, context) { }

        public _.System.RuntimeType? m_runtimeType => Field<_.System.RuntimeType>();

        public _.System.RuntimeType? m_enclosingType => Field<_.System.RuntimeType>();

        public int m_typeCode => Field<int>();

        public string? m_name => Field<string>();

        public string? m_fullName => Field<string>();

        public string? m_assemblyQualifiedName => Field<string>();

        public string? m_toString => Field<string>();

        public string? m_namespace => Field<string>();

        public bool m_isGlobal => Field<bool>();

        public global::_.System.Object? m_methodInfoCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_constructorInfoCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_fieldInfoCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_interfaceCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_nestedClassesCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_propertyInfoCache => Field<global::_.System.Object>();

        public global::_.System.Object? m_eventInfoCache => Field<global::_.System.Object>();

        public string? m_defaultMemberName => Field<string>();

        public global::_.System.Object? m_genericCache => Field<global::_.System.Object>();

        public global::Ndump.Core.DumpArray<_.System.Object?>? _emptyArray => ArrayField<_.System.Object?>();

        public _.System.RuntimeType? _genericTypeDefinition => Field<_.System.RuntimeType>();

        public static new RuntimeTypeCache FromAddress(ulong address, DumpContext context)
            => new RuntimeTypeCache(address, context);

        public static new global::System.Collections.Generic.IEnumerable<RuntimeTypeCache> GetInstances(DumpContext context)
        {
            foreach (var addr in context.EnumerateInstances("System.RuntimeType+RuntimeTypeCache"))
                yield return new RuntimeTypeCache(addr, context);
        }

        public override string ToString() => $"RuntimeTypeCache@0x{_objAddress:X}";
    }

}
