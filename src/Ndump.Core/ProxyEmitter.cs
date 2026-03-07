using System.Text;

namespace Ndump.Core;

/// <summary>
/// Generates C# proxy source files for types discovered from a memory dump.
/// </summary>
public sealed class ProxyEmitter
{
    private const string GeneratedNamespace = "Ndump.Generated";

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
            var safeName = type.Name.Replace('<', '_').Replace('>', '_').Replace(',', '_');
            var filePath = Path.Combine(outputDirectory, $"{safeName}.g.cs");
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
        sb.AppendLine($"namespace {GeneratedNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public sealed class {SanitizeTypeName(type.Name)}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly ulong _objAddress;");
        sb.AppendLine("    private readonly DumpContext _ctx;");
        sb.AppendLine();
        sb.AppendLine($"    private {SanitizeTypeName(type.Name)}(ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("    }");

        // Properties for each field
        foreach (var field in type.Fields)
        {
            sb.AppendLine();
            EmitProperty(sb, type, field);
        }

        // FromAddress factory
        sb.AppendLine();
        sb.AppendLine($"    public static {SanitizeTypeName(type.Name)} FromAddress(ulong address, DumpContext ctx)");
        sb.AppendLine($"        => new {SanitizeTypeName(type.Name)}(address, ctx);");

        // GetInstances
        sb.AppendLine();
        sb.AppendLine($"    public static System.Collections.Generic.IEnumerable<{SanitizeTypeName(type.Name)}> GetInstances(DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        sb.AppendLine($"            yield return new {SanitizeTypeName(type.Name)}(addr, ctx);");
        sb.AppendLine("    }");

        // Address accessor (method to avoid collisions with object fields)
        sb.AppendLine();
        sb.AppendLine("    public ulong GetObjAddress() => _objAddress;");

        // ToString override
        sb.AppendLine();
        sb.AppendLine($"    public override string ToString() => $\"{SanitizeTypeName(type.Name)}@0x{{_objAddress:X}}\";");

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
                    var proxyTypeName = SanitizeTypeName(ExtractTypeName(field.ReferenceTypeName));
                    sb.AppendLine($"    public {proxyTypeName}? {propName}");
                    sb.AppendLine("    {");
                    sb.AppendLine("        get");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var addr = _ctx.GetObjectAddress(_objAddress, \"{ownerType.FullName}\", \"{field.Name}\");");
                    sb.AppendLine($"            return addr == 0 ? null : {proxyTypeName}.FromAddress(addr, _ctx);");
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

    private static string SanitizeTypeName(string name)
    {
        // Handle nested/generic type names
        return name
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '_')
            .Replace(' ', '_')
            .Replace('+', '_')
            .Replace('.', '_');
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

    private static string ExtractTypeName(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot > 0 ? fullName[(lastDot + 1)..] : fullName;
    }
}
