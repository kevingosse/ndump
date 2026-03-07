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

        foreach (var obj in context.Heap.EnumerateObjects())
        {
            if (!obj.IsValid || obj.Type is null) continue;

            var clrType = obj.Type;
            if (clrType.Name is null) continue;
            if (!seen.Add(clrType.Name)) continue;
            if (ShouldExclude(clrType.Name)) continue;
            if (clrType.IsArray) continue;

            var fields = new List<FieldInfo>();
            foreach (var f in clrType.Fields)
            {
                fields.Add(MapField(f));
            }

            var ns = ExtractNamespace(clrType.Name);
            var name = ExtractTypeName(clrType.Name);

            result.Add(new TypeMetadata
            {
                FullName = clrType.Name,
                Namespace = ns,
                Name = name,
                Fields = fields,
                BaseTypeName = clrType.BaseType?.Name == "System.Object" ? null : clrType.BaseType?.Name
            });
        }

        return result;
    }

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
        return new FieldInfo
        {
            Name = field.Name ?? "<unknown>",
            TypeName = MapClrTypeToCs(field, kind),
            Kind = kind,
            ReferenceTypeName = kind == FieldKind.ObjectReference ? field.Type?.Name : null
        };
    }

    private static FieldKind ClassifyField(ClrInstanceField field)
    {
        if (field.Type is null)
            return FieldKind.Unknown;

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
