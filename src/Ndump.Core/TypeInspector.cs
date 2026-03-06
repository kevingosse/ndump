using Microsoft.Diagnostics.Runtime;

namespace Ndump.Core;

/// <summary>
/// Inspects the CLR heap to discover user-defined types and their fields.
/// </summary>
public sealed class TypeInspector
{
    // Prefixes of types we skip (framework/runtime types)
    private static readonly string[] ExcludedPrefixes =
    [
        "System.",
        "Microsoft.",
        "Internal.",
        "Interop.",
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

        if (kind == FieldKind.Primitive && field.Type is not null)
        {
            return field.Type.Name switch
            {
                "System.Boolean" => "bool",
                "System.Byte" => "byte",
                "System.SByte" => "sbyte",
                "System.Int16" => "short",
                "System.UInt16" => "ushort",
                "System.Int32" => "int",
                "System.UInt32" => "uint",
                "System.Int64" => "long",
                "System.UInt64" => "ulong",
                "System.Single" => "float",
                "System.Double" => "double",
                "System.Char" => "char",
                "System.IntPtr" => "nint",
                "System.UIntPtr" => "nuint",
                _ => field.Type.Name ?? "object"
            };
        }

        // For object references, return the type name (will be mapped to proxy type)
        if (kind == FieldKind.ObjectReference && field.Type is not null)
            return field.Type.Name ?? "object";

        return "object";
    }

    private static string ExtractNamespace(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot > 0 ? fullName[..lastDot] : "";
    }

    private static string ExtractTypeName(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot > 0 ? fullName[(lastDot + 1)..] : fullName;
    }
}
