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
    /// Emit .cs files for all discovered types into the given output directory.
    /// Returns the list of generated file paths.
    /// </summary>
    public IReadOnlyList<string> EmitProxies(IReadOnlyList<TypeMetadata> types, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        _knownProxyTypes = new HashSet<string>(types.Select(t => t.FullName));
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
    public string GenerateProxyCode(TypeMetadata type, ISet<string>? knownTypes = null)
    {
        _knownProxyTypes = knownTypes is not null
            ? new HashSet<string>(knownTypes)
            : new HashSet<string> { type.FullName };

        return GenerateProxy(type);
    }

    private string GenerateProxy(TypeMetadata type)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();

        var proxyNamespace = GetProxyNamespace(type.Namespace);
        sb.AppendLine($"namespace {proxyNamespace};");
        sb.AppendLine();

        var sanitizedName = SanitizeTypeName(type.Name);
        sb.AppendLine($"public sealed class {sanitizedName}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ulong _objAddress;");
        sb.AppendLine("    private readonly DumpContext _ctx;");
        sb.AppendLine();
        sb.AppendLine($"    private {sanitizedName}(ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("    }");

        // Properties for each field (deduplicate property names)
        var usedNames = new HashSet<string> { sanitizedName, "_objAddress", "_ctx" };
        foreach (var field in type.Fields)
        {
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

        // FromAddress factory
        sb.AppendLine();
        sb.AppendLine($"    public static {sanitizedName} FromAddress(ulong address, DumpContext ctx)");
        sb.AppendLine($"        => new {sanitizedName}(address, ctx);");

        // GetInstances
        sb.AppendLine();
        sb.AppendLine($"    public static global::System.Collections.Generic.IEnumerable<{sanitizedName}> GetInstances(DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        sb.AppendLine($"            yield return new {sanitizedName}(addr, ctx);");
        sb.AppendLine("    }");

        // Address accessor (method to avoid collisions with object fields)
        sb.AppendLine();
        sb.AppendLine("    public ulong GetObjAddress() => _objAddress;");

        // ToString override
        sb.AppendLine();
        sb.AppendLine($"    public override string ToString() => $\"{sanitizedName}@0x{{_objAddress:X}}\";");

        sb.AppendLine("}");
        return sb.ToString();
    }

    private void EmitProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field)
    {
        var propName = SanitizePropertyName(field.Name);

        switch (field.Kind)
        {
            case FieldKind.String:
                sb.AppendLine($"    public string? {propName} => _ctx.GetStringField(_objAddress, \"{ownerType.FullName}\", \"{field.Name}\");");
                break;

            case FieldKind.Primitive:
                var csType = field.TypeName;
                sb.AppendLine($"    public {csType} {propName} => _ctx.GetFieldValue<{csType}>(_objAddress, \"{ownerType.FullName}\", \"{field.Name}\");");
                break;

            case FieldKind.ObjectReference:
                if (field.ReferenceTypeName is not null && _knownProxyTypes.Contains(field.ReferenceTypeName))
                {
                    var qualifiedProxyType = GetFullyQualifiedProxyType(field.ReferenceTypeName);
                    sb.AppendLine($"    public {qualifiedProxyType}? {propName}");
                    sb.AppendLine("    {");
                    sb.AppendLine("        get");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var addr = _ctx.GetObjectAddress(_objAddress, \"{ownerType.FullName}\", \"{field.Name}\");");
                    sb.AppendLine($"            return addr == 0 ? null : {qualifiedProxyType}.FromAddress(addr, _ctx);");
                    sb.AppendLine("        }");
                    sb.AppendLine("    }");
                }
                else
                {
                    // Unknown reference type — expose as address
                    sb.AppendLine($"    public ulong {propName} => _ctx.GetObjectAddress(_objAddress, \"{ownerType.FullName}\", \"{field.Name}\");");
                }
                break;

            case FieldKind.ValueType:
                // Value types are complex; for now expose as object via GetFieldValue with appropriate struct type
                sb.AppendLine($"    // ValueType field: {field.Name} ({field.TypeName}) — not yet supported");
                break;

            default:
                sb.AppendLine($"    // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
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
