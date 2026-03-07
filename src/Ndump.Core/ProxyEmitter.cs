using System.Text;

namespace Ndump.Core;

/// <summary>
/// Generates C# proxy source files for types discovered from a memory dump.
/// </summary>
public sealed class ProxyEmitter
{
    /// <summary>
    /// Set of fully-qualified type names that have corresponding proxies being generated.
    /// Used to determine if a reference field can resolve to a proxy type.
    /// </summary>
    private HashSet<string> _knownProxyTypes = [];

    /// <summary>
    /// Lookup from full CLR name to TypeMetadata, for resolving base type field lists.
    /// </summary>
    private Dictionary<string, TypeMetadata> _typesByName = [];

    /// <summary>
    /// CLR type names that serve as bases for other known proxy types.
    /// These must not be sealed so derived proxies can extend them.
    /// </summary>
    private HashSet<string> _baseTypes = [];

    /// <summary>
    /// Emit .cs files for all discovered types into the given output directory.
    /// Returns the list of generated file paths.
    /// </summary>
    public IReadOnlyList<string> EmitProxies(IReadOnlyList<TypeMetadata> types, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        SetupContext(types);
        var files = new List<string>();

        foreach (var type in types)
        {
            var code = GenerateProxy(type);
            // Include namespace in the filename to avoid collisions
            var nsPrefix = string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace.Replace('.', '_') + "_";
            var safeName = SanitizeTypeName(type.Name);
            var filePath = Path.Combine(outputDirectory, $"{nsPrefix}{safeName}.g.cs");
            File.WriteAllText(filePath, code);
            files.Add(filePath);
        }

        return files;
    }

    /// <summary>
    /// Generate proxy source code for a given type.
    /// Useful for unit testing without writing to disk.
    /// </summary>
    public string GenerateProxyCode(TypeMetadata type, ISet<string>? knownTypes = null, IReadOnlyList<TypeMetadata>? allTypes = null)
    {
        if (allTypes is not null)
        {
            SetupContext(allTypes);
        }
        else
        {
            _knownProxyTypes = knownTypes is not null
                ? new HashSet<string>(knownTypes)
                : new HashSet<string> { type.FullName };
            _typesByName = [];
            _baseTypes = [];
        }

        return GenerateProxy(type);
    }

    private void SetupContext(IReadOnlyList<TypeMetadata> types)
    {
        _knownProxyTypes = new HashSet<string>(types.Select(t => t.FullName));
        _typesByName = types.ToDictionary(t => t.FullName);
        _baseTypes = new HashSet<string>(
            types.Where(t => t.BaseTypeName is not null && _knownProxyTypes.Contains(t.BaseTypeName))
                 .Select(t => t.BaseTypeName!));
    }

    private string GenerateProxy(TypeMetadata type)
    {
        if (type.FullName == "System.Object")
            return GenerateSystemObjectProxy(type);

        var sb = new StringBuilder();
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();

        var proxyNamespace = GetProxyNamespace(type.Namespace);
        sb.AppendLine($"namespace {proxyNamespace};");
        sb.AppendLine();

        var sanitizedName = SanitizeTypeName(type.Name);

        // Determine base class: use proxy of CLR base type if known, otherwise _.System.Object
        var hasKnownBase = type.BaseTypeName is not null && _knownProxyTypes.Contains(type.BaseTypeName);
        var baseClass = hasKnownBase
            ? GetFullyQualifiedProxyType(type.BaseTypeName!)
            : "global::_.System.Object";

        // Don't seal types that serve as bases for other proxies
        var isSealed = !_baseTypes.Contains(type.FullName);
        var sealedKeyword = isSealed ? "sealed " : "";

        sb.AppendLine($"public {sealedKeyword}class {sanitizedName} : {baseClass}");
        sb.AppendLine("{");
        var ctorAccess = isSealed ? "private" : "protected";
        sb.AppendLine($"    {ctorAccess} {sanitizedName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");

        // Collect field names from the base type to skip inherited fields
        var baseFieldNames = CollectBaseFieldNames(type);

        // Properties for each field (deduplicate property names, skip inherited)
        var usedNames = new HashSet<string> { sanitizedName, "_objAddress", "_ctx" };
        foreach (var field in type.Fields)
        {
            if (baseFieldNames.Contains(field.Name))
                continue;

            var propName = SanitizePropertyName(field.Name);
            if (!usedNames.Add(propName))
            {
                // Skip duplicate property names (can happen with inheritance/hidden fields)
                sb.AppendLine();
                sb.AppendLine($"    // Duplicate field skipped: {field.Name} ({field.TypeName})");
                continue;
            }

            sb.AppendLine();
            EmitProperty(sb, type, field);
        }

        // FromAddress factory — use 'new' keyword if base also has FromAddress
        var newKeyword = hasKnownBase ? "new " : "";
        sb.AppendLine();
        sb.AppendLine($"    public static {newKeyword}{sanitizedName} FromAddress(ulong address, DumpContext ctx)");
        sb.AppendLine($"        => new {sanitizedName}(address, ctx);");

        // GetInstances
        sb.AppendLine();
        sb.AppendLine($"    public static {newKeyword}global::System.Collections.Generic.IEnumerable<{sanitizedName}> GetInstances(DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        sb.AppendLine($"            yield return new {sanitizedName}(addr, ctx);");
        sb.AppendLine("    }");

        // ToString override
        sb.AppendLine();
        sb.AppendLine($"    public override string ToString() => $\"{sanitizedName}@0x{{_objAddress:X}}\";");

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Generate the root System.Object proxy that declares _objAddress, _ctx, and GetObjAddress().
    /// All other proxies ultimately inherit from this type.
    /// </summary>
    private static string GenerateSystemObjectProxy(TypeMetadata type)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();
        sb.AppendLine("namespace _.System;");
        sb.AppendLine();
        sb.AppendLine("public class Object");
        sb.AppendLine("{");
        sb.AppendLine("    protected readonly ulong _objAddress;");
        sb.AppendLine("    protected readonly DumpContext _ctx;");
        sb.AppendLine();
        sb.AppendLine("    protected Object(ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public ulong GetObjAddress() => _objAddress;");
        sb.AppendLine();
        sb.AppendLine("    public static Object FromAddress(ulong address, DumpContext ctx)");
        sb.AppendLine("        => new Object(address, ctx);");
        sb.AppendLine();
        sb.AppendLine("    public static global::System.Collections.Generic.IEnumerable<Object> GetInstances(DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        sb.AppendLine("            yield return new Object(addr, ctx);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public override string ToString() => $\"Object@0x{_objAddress:X}\";");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private HashSet<string> CollectBaseFieldNames(TypeMetadata type)
    {
        var result = new HashSet<string>();
        var current = type.BaseTypeName;
        while (current is not null && _typesByName.TryGetValue(current, out var baseMeta))
        {
            foreach (var f in baseMeta.Fields)
                result.Add(f.Name);
            current = baseMeta.BaseTypeName;
        }
        return result;
    }

    private void EmitProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field)
    {
        var propName = SanitizePropertyName(field.Name);

        switch (field.Kind)
        {
            case FieldKind.String:
                sb.AppendLine($"    public string? {propName} => _ctx.GetStringField(_objAddress, \"{field.Name}\");");
                break;

            case FieldKind.Primitive:
                var csType = field.TypeName;
                sb.AppendLine($"    public {csType} {propName} => _ctx.GetFieldValue<{csType}>(_objAddress, \"{field.Name}\");");
                break;

            case FieldKind.ObjectReference:
                EmitObjectReferenceProperty(sb, ownerType, field, propName);
                break;

            case FieldKind.Array:
                EmitArrayProperty(sb, ownerType, field, propName);
                break;

            case FieldKind.ValueType:
                sb.AppendLine($"    // ValueType field: {field.Name} ({field.TypeName}) — not yet supported");
                break;

            default:
                sb.AppendLine($"    // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
    }

    private static bool IsUsableProxyType(string? typeName, HashSet<string> knownTypes)
    {
        return typeName is not null && knownTypes.Contains(typeName);
    }

    private void EmitObjectReferenceProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName)
    {
        if (IsUsableProxyType(field.ReferenceTypeName, _knownProxyTypes))
        {
            var qualifiedProxyType = GetFullyQualifiedProxyType(field.ReferenceTypeName!);
            var isBaseType = _baseTypes.Contains(field.ReferenceTypeName!);
            sb.AppendLine($"    public {qualifiedProxyType}? {propName}");
            sb.AppendLine("    {");
            sb.AppendLine("        get");
            sb.AppendLine("        {");
            sb.AppendLine($"            var addr = _ctx.GetObjectAddress(_objAddress, \"{field.Name}\");");
            if (isBaseType)
                sb.AppendLine($"            return addr == 0 ? null : _ctx.ResolveProxy(addr) as {qualifiedProxyType} ?? {qualifiedProxyType}.FromAddress(addr, _ctx);");
            else
                sb.AppendLine($"            return addr == 0 ? null : {qualifiedProxyType}.FromAddress(addr, _ctx);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
        else
        {
            // Unknown reference type — expose as address
            sb.AppendLine($"    public ulong {propName} => _ctx.GetObjectAddress(_objAddress, \"{field.Name}\");");
        }
    }

    private void EmitArrayProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName)
    {
        var elementKind = field.ArrayElementKind ?? FieldKind.Unknown;
        var elementTypeName = field.ArrayElementTypeName;

        // Determine the C# element type and the reader lambda
        string csElementType;
        string readerLambda;

        switch (elementKind)
        {
            case FieldKind.String:
                csElementType = "string?";
                readerLambda = "i => _ctx.GetArrayElementString(addr, i)";
                break;

            case FieldKind.Primitive:
                csElementType = MapClrPrimitiveTypeName(elementTypeName);
                readerLambda = $"i => _ctx.GetArrayElementValue<{csElementType}>(addr, i)";
                break;

            case FieldKind.ObjectReference:
                if (IsUsableProxyType(elementTypeName, _knownProxyTypes))
                {
                    var qualifiedProxy = GetFullyQualifiedProxyType(elementTypeName!);
                    csElementType = $"{qualifiedProxy}?";
                    var isBaseType = _baseTypes.Contains(elementTypeName!);
                    if (isBaseType)
                        readerLambda = $"i => {{ var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : _ctx.ResolveProxy(ea) as {qualifiedProxy} ?? {qualifiedProxy}.FromAddress(ea, _ctx); }}";
                    else
                        readerLambda = $"i => {{ var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : {qualifiedProxy}.FromAddress(ea, _ctx); }}";
                }
                else
                {
                    // Unknown element type — expose as address
                    csElementType = "ulong";
                    readerLambda = "i => _ctx.GetArrayElementAddress(addr, i)";
                }
                break;

            default:
                sb.AppendLine($"    // Array field: {field.Name} ({field.TypeName}) — element type not supported");
                return;
        }

        sb.AppendLine($"    public global::Ndump.Core.DumpArray<{csElementType}>? {propName}");
        sb.AppendLine("    {");
        sb.AppendLine("        get");
        sb.AppendLine("        {");
        sb.AppendLine($"            var addr = _ctx.GetObjectAddress(_objAddress, \"{field.Name}\");");
        sb.AppendLine("            if (addr == 0) return null;");
        sb.AppendLine($"            var len = _ctx.GetArrayLength(addr);");
        sb.AppendLine($"            return new global::Ndump.Core.DumpArray<{csElementType}>(addr, len, {readerLambda});");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    private static string MapClrPrimitiveTypeName(string? clrTypeName)
    {
        return clrTypeName switch
        {
            "System.Boolean" => "bool",
            "System.Char" => "char",
            "System.SByte" => "sbyte",
            "System.Byte" => "byte",
            "System.Int16" => "short",
            "System.UInt16" => "ushort",
            "System.Int32" => "int",
            "System.UInt32" => "uint",
            "System.Int64" => "long",
            "System.UInt64" => "ulong",
            "System.Single" => "float",
            "System.Double" => "double",
            "System.IntPtr" => "nint",
            "System.UIntPtr" => "nuint",
            _ => "int"
        };
    }

    /// <summary>
    /// Get the proxy namespace for a given original namespace.
    /// E.g., "System.Text" → "_.System.Text", "" → "_"
    /// </summary>
    internal static string GetProxyNamespace(string originalNamespace)
    {
        return string.IsNullOrEmpty(originalNamespace) ? "_" : $"_.{originalNamespace}";
    }

    /// <summary>
    /// Get the fully qualified proxy type name (for reflection/type lookup).
    /// E.g., TypeMetadata { Namespace = "MyApp", Name = "Customer" } → "_.MyApp.Customer"
    /// </summary>
    public static string GetProxyTypeName(TypeMetadata type)
    {
        return $"{GetProxyNamespace(type.Namespace)}.{SanitizeTypeName(type.Name)}";
    }

    /// <summary>
    /// Get the fully qualified proxy type name for a given CLR full type name.
    /// E.g., "MyApp.Customer" → "_.MyApp.Customer"
    /// </summary>
    private static string GetFullyQualifiedProxyType(string fullTypeName)
    {
        // Find split point before any generic markers (<, `, [)
        var genericStart = fullTypeName.Length;
        for (int i = 0; i < fullTypeName.Length; i++)
        {
            if (fullTypeName[i] is '<' or '`' or '[')
            {
                genericStart = i;
                break;
            }
        }

        var lastDot = fullTypeName.LastIndexOf('.', genericStart - 1);
        if (lastDot > 0)
        {
            var ns = fullTypeName[..lastDot];
            var name = fullTypeName[(lastDot + 1)..];
            return $"_.{ns}.{SanitizeTypeName(name)}";
        }
        return $"_.{SanitizeTypeName(fullTypeName)}";
    }

    internal static string SanitizeTypeName(string name)
    {
        // Handle nested/generic type names
        return name
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '_')
            .Replace(' ', '_')
            .Replace('+', '_')
            .Replace('.', '_')
            .Replace('`', '_')
            .Replace('[', '_')
            .Replace(']', '_');
    }

    private static string SanitizePropertyName(string fieldName)
    {
        // Keep the original field name as the property name (including leading underscores)
        // but make it a valid C# identifier
        if (string.IsNullOrEmpty(fieldName) || fieldName == "<unknown>")
            return "_unknown";

        // Handle backing fields like <PropertyName>k__BackingField
        if (fieldName.StartsWith('<') && fieldName.Contains(">k__BackingField"))
        {
            var propName = fieldName[1..fieldName.IndexOf('>')];
            return propName;
        }

        return fieldName
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(' ', '_');
    }
}
