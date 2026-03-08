using Microsoft.Diagnostics.Runtime;

namespace Ndump.Core;

/// <summary>
/// Inspects the CLR heap to discover user-defined types and their fields.
/// </summary>
public sealed class TypeInspector
{
    // Prefixes of types we skip (compiler-generated / internal runtime types)
    private static readonly string[] ExcludedPrefixes =
    [
        "<>",
        "<Module>",
        "<PrivateImplementationDetails>"
    ];

    /// <summary>
    /// Discover all user-defined reference types on the heap.
    /// </summary>
    public IReadOnlyList<TypeMetadata> DiscoverTypes(DumpContext context)
    {
        var seen = new HashSet<string>();
        var result = new List<TypeMetadata>();
        // Value types discovered from array element types — process after heap walk
        var pendingValueTypes = new List<ClrType>();

        foreach (var obj in context.Heap.EnumerateObjects())
        {
            if (!obj.IsValid || obj.Type is null) continue;

            // Discover value types from array component types
            if (obj.Type.IsArray && obj.Type.ComponentType is { IsValueType: true } compType
                && compType.Name is not null && !ShouldExclude(compType.Name))
            {
                pendingValueTypes.Add(compType);
            }

            // Walk the type and its base types to discover abstract/base types too
            var clrType = obj.Type;
            while (clrType is not null && clrType.Name is not null)
            {
                if (!seen.Add(clrType.Name) || ShouldExclude(clrType.Name) || clrType.IsArray)
                {
                    clrType = clrType.BaseType;
                    continue;
                }

                result.Add(BuildTypeMetadata(clrType, isValueType: false));

                // Discover value types used as fields
                foreach (var field in clrType.Fields)
                {
                    if (field.IsValueType && field.Type is { IsValueType: true, IsPrimitive: false, Name: not null }
                        && field.Type.Name != "System.String" && !ShouldExclude(field.Type.Name)
                        && !IsExcludedValueType(field.Type.Name)
                        && !IsNullableType(field.Type.Name))
                    {
                        pendingValueTypes.Add(field.Type);
                    }
                }

                clrType = clrType.BaseType;
            }
        }

        // Now process discovered value types
        foreach (var vt in pendingValueTypes)
        {
            if (!seen.Add(vt.Name!)) continue;
            result.Add(BuildTypeMetadata(vt, isValueType: true));
        }

        return result;
    }

    private static TypeMetadata BuildTypeMetadata(ClrType clrType, bool isValueType)
    {
        var fields = new List<FieldInfo>();
        foreach (var f in clrType.Fields)
        {
            fields.Add(MapField(f));
        }

        var ns = ExtractNamespace(clrType.Name!);
        var name = ExtractTypeName(clrType.Name!);

        var (genDefName, genArgs) = ParseGenericName(name);
        string? genDefFullName = null;
        if (genDefName is not null)
        {
            var anglePos = FindGenericAngleInFullName(clrType.Name!);
            genDefFullName = anglePos > 0 ? clrType.Name![..anglePos] : null;
            if (genDefFullName is null) genDefName = null;
        }

        return new TypeMetadata
        {
            FullName = clrType.Name!,
            Namespace = ns,
            Name = name,
            Fields = fields,
            BaseTypeName = clrType.BaseType?.Name,
            GenericDefinitionName = genDefName,
            GenericDefinitionFullName = genDefFullName,
            GenericTypeArguments = genArgs,
            IsValueType = isValueType
        };
    }

    /// <summary>
    /// Value types that should not be discovered as struct proxies from field types.
    /// These are either meaningless (Void, ValueType) or already handled as primitives.
    /// </summary>
    private static bool IsExcludedValueType(string typeName) => typeName is
        "System.Void" or "System.ValueType" or
        "System.Boolean" or "System.Char" or
        "System.SByte" or "System.Byte" or
        "System.Int16" or "System.UInt16" or
        "System.Int32" or "System.UInt32" or
        "System.Int64" or "System.UInt64" or
        "System.Single" or "System.Double" or
        "System.IntPtr" or "System.UIntPtr";

    private static bool ShouldExclude(string typeName)
    {
        foreach (var prefix in ExcludedPrefixes)
        {
            if (typeName.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static FieldInfo MapField(ClrInstanceField field)
    {
        var kind = ClassifyField(field);

        if (kind == FieldKind.Array && field.Type?.ComponentType is { } componentType)
        {
            var elementKind = ClassifyComponentType(componentType);
            return new FieldInfo
            {
                Name = field.Name ?? "<unknown>",
                TypeName = field.Type.Name ?? "object[]",
                Kind = kind,
                ArrayElementTypeName = componentType.Name,
                ArrayElementKind = elementKind
            };
        }

        // Fallback: when ComponentType is null (ClrMD quirk for some primitive arrays),
        // infer element type from the array type name.
        if (kind == FieldKind.Array && field.Type is { Name: not null } arrayType
            && arrayType.Name.EndsWith("[]", StringComparison.Ordinal))
        {
            var elementTypeName = arrayType.Name[..^2];
            var elementKind = InferElementKindFromTypeName(elementTypeName);
            if (elementKind != FieldKind.Unknown)
            {
                return new FieldInfo
                {
                    Name = field.Name ?? "<unknown>",
                    TypeName = arrayType.Name,
                    Kind = kind,
                    ArrayElementTypeName = elementTypeName,
                    ArrayElementKind = elementKind
                };
            }
        }

        // Detect Nullable<T> value type fields — both resolved and unresolved
        if (kind == FieldKind.ValueType && field.Type?.Name is not null
            && IsNullableType(field.Type.Name))
        {
            var resolvedInner = TryResolveNullableInnerType(field);
            if (resolvedInner is not null)
            {
                return new FieldInfo
                {
                    Name = field.Name ?? "<unknown>",
                    TypeName = MapClrTypeToCs(field, kind),
                    Kind = kind,
                    ReferenceTypeName = field.Type.Name,
                    NullableInnerTypeName = resolvedInner
                };
            }
        }

        return new FieldInfo
        {
            Name = field.Name ?? "<unknown>",
            TypeName = MapClrTypeToCs(field, kind),
            Kind = kind,
            ReferenceTypeName = kind is FieldKind.ObjectReference or FieldKind.ValueType ? field.Type?.Name : null
        };
    }

    /// <summary>
    /// For a Nullable&lt;T&gt; field, resolve the concrete inner type.
    /// For resolved names (e.g., System.Nullable&lt;System.DateTime&gt;), extract from the name.
    /// For unresolved names (e.g., System.Nullable&lt;T1&gt;), examine ClrMD sub-fields.
    /// </summary>
    private static string? TryResolveNullableInnerType(ClrInstanceField field)
    {
        if (field.Type is null) return null;

        // If the type name is already resolved (no unresolved params), extract inner type from the name
        if (!HasUnresolvedTypeParams(field.Type.Name!))
        {
            var (_, args) = ParseGenericName(field.Type.Name!);
            if (args.Count == 1) return args[0];
        }

        // Otherwise, probe the "value" sub-field of the Nullable struct
        var valueField = field.Type.GetFieldByName("value");
        if (valueField is null) return null;

        // First check: does the value sub-field have a resolved type name?
        if (valueField.Type?.Name is not null
            && !valueField.Type.Name.Contains("T1") && !valueField.Type.Name.Contains("T2"))
        {
            return valueField.Type.Name;
        }

        // Fallback: use the ElementType to determine the primitive type
        if (valueField.IsPrimitive)
        {
            return MapElementTypeToClrName(valueField.ElementType);
        }

        return null;
    }

    /// <summary>
    /// Check if a type name is a Nullable&lt;T&gt; type.
    /// </summary>
    private static bool IsNullableType(string typeName)
        => typeName.StartsWith("System.Nullable<", StringComparison.Ordinal);

    /// <summary>
    /// Check if a type name has unresolved generic type parameters.
    /// </summary>
    private static bool HasUnresolvedTypeParams(string typeName)
    {
        var angleIdx = typeName.IndexOf('<');
        if (angleIdx < 0) return false;
        var (_, args) = ParseGenericName(typeName);
        return args.Any(a => !a.Contains('.'));
    }

    private static string? MapElementTypeToClrName(ClrElementType elementType)
    {
        return elementType switch
        {
            ClrElementType.Boolean => "System.Boolean",
            ClrElementType.Char => "System.Char",
            ClrElementType.Int8 => "System.SByte",
            ClrElementType.UInt8 => "System.Byte",
            ClrElementType.Int16 => "System.Int16",
            ClrElementType.UInt16 => "System.UInt16",
            ClrElementType.Int32 => "System.Int32",
            ClrElementType.UInt32 => "System.UInt32",
            ClrElementType.Int64 => "System.Int64",
            ClrElementType.UInt64 => "System.UInt64",
            ClrElementType.Float => "System.Single",
            ClrElementType.Double => "System.Double",
            ClrElementType.NativeInt => "System.IntPtr",
            ClrElementType.NativeUInt => "System.UIntPtr",
            _ => null
        };
    }

    private static FieldKind ClassifyField(ClrInstanceField field)
    {
        if (field.Type is null)
        {

            // ClrMD couldn't resolve the type (e.g., field was null and the type
            // was never instantiated on the heap). Use ElementType to classify.
            return field.ElementType switch
            {
                ClrElementType.Class or ClrElementType.Object => FieldKind.ObjectReference,
                ClrElementType.String => FieldKind.String,
                ClrElementType.SZArray or ClrElementType.Array => FieldKind.Array,
                _ => FieldKind.Unknown
            };
        }

        if (field.Type.IsArray)
            return FieldKind.Array;

        if (field.Type.Name == "System.String")
            return FieldKind.String;

        if (field.IsObjectReference)
            return FieldKind.ObjectReference;

        if (field.IsPrimitive)
            return FieldKind.Primitive;

        if (field.IsValueType)
            return FieldKind.ValueType;

        return FieldKind.Unknown;
    }

    /// <summary>
    /// Infer the element FieldKind from a CLR type name when ComponentType is unavailable.
    /// Handles primitives and System.String; returns Unknown for anything else.
    /// </summary>
    private static FieldKind InferElementKindFromTypeName(string typeName)
    {
        if (typeName == "System.String")
            return FieldKind.String;

        if (typeName is
            "System.Boolean" or "System.Char" or
            "System.SByte" or "System.Byte" or
            "System.Int16" or "System.UInt16" or
            "System.Int32" or "System.UInt32" or
            "System.Int64" or "System.UInt64" or
            "System.Single" or "System.Double" or
            "System.IntPtr" or "System.UIntPtr")
        {
            return FieldKind.Primitive;
        }

        return FieldKind.Unknown;
    }

    private static FieldKind ClassifyComponentType(ClrType componentType)
    {
        if (componentType.Name == "System.String")
            return FieldKind.String;

        if (componentType.IsObjectReference)
            return FieldKind.ObjectReference;

        if (componentType.IsPrimitive)
            return FieldKind.Primitive;

        if (componentType.IsValueType)
            return FieldKind.ValueType;

        return FieldKind.Unknown;
    }

    internal static string MapClrTypeToCs(ClrInstanceField field, FieldKind kind)
    {
        if (kind == FieldKind.String)
            return "string";

        if (kind == FieldKind.Primitive)
        {
            // Use ElementType to determine the C# primitive type.
            // This correctly handles enums (maps to their underlying type)
            // and avoids emitting CLR type names that may conflict with proxy namespaces.
            return MapElementType(field.ElementType);
        }

        // For object references, return the type name (will be mapped to proxy type)
        if (kind == FieldKind.ObjectReference && field.Type is not null)
            return field.Type.Name ?? "object";

        return "object";
    }

    private static string MapElementType(ClrElementType elementType)
    {
        return elementType switch
        {
            ClrElementType.Boolean => "bool",
            ClrElementType.Char => "char",
            ClrElementType.Int8 => "sbyte",
            ClrElementType.UInt8 => "byte",
            ClrElementType.Int16 => "short",
            ClrElementType.UInt16 => "ushort",
            ClrElementType.Int32 => "int",
            ClrElementType.UInt32 => "uint",
            ClrElementType.Int64 => "long",
            ClrElementType.UInt64 => "ulong",
            ClrElementType.Float => "float",
            ClrElementType.Double => "double",
            ClrElementType.NativeInt => "nint",
            ClrElementType.NativeUInt => "nuint",
            ClrElementType.Pointer => "nuint",
            ClrElementType.FunctionPointer => "nuint",
            _ => "int" // Safe fallback for unknown primitive element types
        };
    }

    /// <summary>
    /// Find the split point between namespace and type name.
    /// For generic types like "System.Collections.Generic.List&lt;System.String&gt;",
    /// we need to find the last dot that's part of the namespace, not inside generic args.
    /// </summary>
    private static int FindNamespaceSplit(string fullName)
    {
        // Find the start of generic args (first '<', '`', or '[' that indicates generics)
        var genericStart = fullName.Length;
        for (int i = 0; i < fullName.Length; i++)
        {
            if (fullName[i] is '<' or '`' or '[')
            {
                genericStart = i;
                break;
            }
        }

        // Find the last dot before generic args
        var lastDot = fullName.LastIndexOf('.', genericStart - 1);
        return lastDot;
    }

    /// <summary>
    /// Parse a type name to extract the generic definition and type arguments.
    /// E.g., "Dictionary&lt;System.String, System.Object&gt;" → ("Dictionary", ["System.String", "System.Object"])
    /// Non-generic names return (null, []).
    /// </summary>
    /// <summary>
    /// Parse a type name to extract the generic definition and type arguments.
    /// Only considers the leaf part of nested types (after the last + outside generics).
    /// Ignores compiler-generated names like &lt;&gt;c.
    /// </summary>
    internal static (string? DefinitionName, IReadOnlyList<string> TypeArguments) ParseGenericName(string typeName)
    {
        // Find the leaf part of nested types (after last + outside angle brackets)
        int leafStart = 0;
        int d = 0;
        for (int i = 0; i < typeName.Length; i++)
        {
            if (typeName[i] == '<') d++;
            else if (typeName[i] == '>') d--;
            else if (typeName[i] == '+' && d == 0)
                leafStart = i + 1;
        }
        var leaf = typeName[leafStart..];

        // Find the first < in the leaf
        var firstAngle = leaf.IndexOf('<');
        if (firstAngle < 0)
            return (null, []);

        // Skip compiler-generated names: leaf starts with < (like <>c), or <> empty brackets
        if (firstAngle == 0)
            return (null, []);
        if (firstAngle + 1 < leaf.Length && leaf[firstAngle + 1] == '>')
            return (null, []);

        var defName = leaf[..firstAngle];

        // Parse type arguments (balanced brackets)
        var args = new List<string>();
        var depth = 0;
        var start = firstAngle + 1;
        for (int i = firstAngle; i < leaf.Length; i++)
        {
            if (leaf[i] == '<') depth++;
            else if (leaf[i] == '>')
            {
                depth--;
                if (depth == 0)
                {
                    args.Add(leaf[start..i].Trim());
                    break;
                }
            }
            else if (leaf[i] == ',' && depth == 1)
            {
                args.Add(leaf[start..i].Trim());
                start = i + 1;
            }
        }

        return (defName, args);
    }

    /// <summary>
    /// Find the position of the &lt; that starts the generic type arguments in the leaf part
    /// of a full CLR type name. Skips compiler-generated &lt;&gt; patterns.
    /// Returns -1 if not found.
    /// </summary>
    private static int FindGenericAngleInFullName(string fullName)
    {
        // Walk the string, tracking + nesting depth for angle brackets
        int d = 0;
        int lastPlus = -1;
        for (int i = 0; i < fullName.Length; i++)
        {
            if (fullName[i] == '<') d++;
            else if (fullName[i] == '>') d--;
            else if (fullName[i] == '+' && d == 0) lastPlus = i;
        }

        // Find the first < in the leaf part that isn't a <> compiler pattern
        var searchStart = lastPlus + 1;
        for (int i = searchStart; i < fullName.Length; i++)
        {
            if (fullName[i] == '<')
            {
                // Skip if it's at the start of the leaf (compiler name like <>c)
                if (i == searchStart) return -1;
                // Skip if it's an empty <> (compiler pattern)
                if (i + 1 < fullName.Length && fullName[i + 1] == '>') return -1;
                return i;
            }
        }
        return -1;
    }

    private static string ExtractNamespace(string fullName)
    {
        var lastDot = FindNamespaceSplit(fullName);
        return lastDot > 0 ? fullName[..lastDot] : "";
    }

    private static string ExtractTypeName(string fullName)
    {
        var lastDot = FindNamespaceSplit(fullName);
        return lastDot > 0 ? fullName[(lastDot + 1)..] : fullName;
    }
}
