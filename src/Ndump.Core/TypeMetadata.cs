namespace Ndump.Core;

/// <summary>
/// Metadata about a single field on a type discovered from the dump.
/// </summary>
public sealed class FieldInfo
{
    public required string Name { get; init; }
    public required string TypeName { get; init; }
    public required FieldKind Kind { get; init; }

    /// <summary>
    /// For reference types, the full CLR type name (used to resolve to the generated proxy type).
    /// </summary>
    public string? ReferenceTypeName { get; init; }

    /// <summary>
    /// For array fields, the full CLR type name of the array element.
    /// </summary>
    public string? ArrayElementTypeName { get; init; }

    /// <summary>
    /// For array fields, the kind of the array element.
    /// </summary>
    public FieldKind? ArrayElementKind { get; init; }
}

public enum FieldKind
{
    Primitive,
    String,
    ObjectReference,
    ValueType,
    Array,
    Unknown
}

/// <summary>
/// Metadata about a type discovered from the dump.
/// </summary>
public sealed class TypeMetadata
{
    public required string FullName { get; init; }
    public required string Namespace { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<FieldInfo> Fields { get; init; }
    public string? BaseTypeName { get; init; }

    /// <summary>
    /// For generic types, the definition name without type arguments.
    /// E.g., "Dictionary" for "Dictionary&lt;System.String, System.Object&gt;".
    /// Null for non-generic types.
    /// </summary>
    public string? GenericDefinitionName { get; init; }

    /// <summary>
    /// For generic types, the fully-qualified definition name including namespace.
    /// E.g., "System.Collections.Generic.Dictionary" for "System.Collections.Generic.Dictionary&lt;System.String, System.Object&gt;".
    /// Null for non-generic types.
    /// </summary>
    public string? GenericDefinitionFullName { get; init; }

    /// <summary>
    /// For generic types, the ordered list of type argument CLR names.
    /// E.g., ["System.String", "System.Object"] for Dictionary&lt;System.String, System.Object&gt;.
    /// Empty for non-generic types.
    /// </summary>
    public IReadOnlyList<string> GenericTypeArguments { get; init; } = [];

    public bool IsGenericInstance => GenericDefinitionName is not null;
}
