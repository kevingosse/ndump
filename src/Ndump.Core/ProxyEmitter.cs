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
    /// CLR type names that serve as outer (container) types for nested proxy types.
    /// These must be declared as partial so the nested type can be added in a separate file.
    /// </summary>
    private HashSet<string> _nestingContainerTypes = [];

    /// <summary>
    /// Fully-qualified generic definition names that have proxy classes.
    /// E.g., "System.Collections.Generic.Dictionary" for Dictionary&lt;TKey, TValue&gt;.
    /// </summary>
    private HashSet<string> _knownGenericDefinitions = [];

    /// <summary>
    /// Maps generic definition full name to the first specialization (used as template for field layout).
    /// </summary>
    private Dictionary<string, TypeMetadata> _genericRepresentatives = [];

    /// <summary>
    /// Emit .cs files for all discovered types into the given output directory.
    /// Returns the list of generated file paths.
    /// </summary>
    public IReadOnlyList<string> EmitProxies(IReadOnlyList<TypeMetadata> types, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        SetupContext(types);
        var files = new List<string>();
        var emittedGenericDefs = new HashSet<string>();

        foreach (var type in types)
        {
            string code;
            string safeName;

            if (type.IsGenericInstance)
            {
                // Emit one generic proxy per definition, skip subsequent specializations
                if (!emittedGenericDefs.Add(type.GenericDefinitionFullName!))
                    continue;
                code = GenerateGenericProxy(type);
                safeName = type.GenericDefinitionName! + $"_{type.GenericTypeArguments.Count}";
            }
            else
            {
                code = GenerateProxy(type);
                safeName = SanitizeTypeName(type.Name);
            }

            // Include namespace in the filename to avoid collisions
            var nsPrefix = string.IsNullOrEmpty(type.Namespace) ? "" : type.Namespace.Replace('.', '_') + "_";
            var filePath = Path.Combine(outputDirectory, $"{nsPrefix}{safeName}.g.cs");
            File.WriteAllText(filePath, code);
            files.Add(filePath);
        }

        // Emit the resolver that enables polymorphic field/array resolution
        var resolverPath = Path.Combine(outputDirectory, "ProxyResolver.g.cs");
        File.WriteAllText(resolverPath, GenerateProxyResolver());
        files.Add(resolverPath);

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
                ? new HashSet<string>(knownTypes.Where(n => !n.Contains('<')))
                : type.IsGenericInstance ? [] : new HashSet<string> { type.FullName };
            _typesByName = [];
            _baseTypes = [];
            _nestingContainerTypes = [];
            _knownGenericDefinitions = [];
            _genericRepresentatives = [];

            // If the type itself is generic, set up its definition as known
            if (type.IsGenericInstance)
            {
                _knownGenericDefinitions.Add(type.GenericDefinitionFullName!);
                _genericRepresentatives[type.GenericDefinitionFullName!] = type;
            }

            // Also check knownTypes for generic definitions
            if (knownTypes is not null)
            {
                foreach (var name in knownTypes)
                {
                    var genDef = ExtractGenericDefinitionFullName(name);
                    if (genDef is not null)
                        _knownGenericDefinitions.Add(genDef);
                }
            }
        }

        if (type.IsGenericInstance)
            return GenerateGenericProxy(type);

        return GenerateProxy(type);
    }

    private void SetupContext(IReadOnlyList<TypeMetadata> types)
    {
        _knownProxyTypes = new HashSet<string>(types.Where(t => !t.IsGenericInstance).Select(t => t.FullName));
        _typesByName = types.ToDictionary(t => t.FullName);
        _baseTypes = new HashSet<string>(
            types.Where(t => t.BaseTypeName is not null && _knownProxyTypes.Contains(t.BaseTypeName))
                 .Select(t => t.BaseTypeName!));

        // Find types that are nesting containers for other known nested types
        _nestingContainerTypes = [];
        foreach (var type in types)
        {
            var parts = SplitNestingParts(type.Name);
            if (parts.Length <= 1) continue;

            for (int depth = 0; depth < parts.Length - 1; depth++)
            {
                var outerName = string.Join("+", parts[..(depth + 1)]);
                var outerFullName = string.IsNullOrEmpty(type.Namespace) ? outerName : $"{type.Namespace}.{outerName}";
                if (_knownProxyTypes.Contains(outerFullName))
                    _nestingContainerTypes.Add(outerFullName);
            }
        }

        // Group generic types by definition and pick a representative for each
        _knownGenericDefinitions = [];
        _genericRepresentatives = [];
        foreach (var group in types.Where(t => t.IsGenericInstance).GroupBy(t => t.GenericDefinitionFullName!))
        {
            _knownGenericDefinitions.Add(group.Key);
            _genericRepresentatives[group.Key] = group.First();
        }
    }

    private string GenerateProxy(TypeMetadata type)
    {
        if (type.FullName == "System.Object")
            return GenerateSystemObjectProxy(type);

        if (type.FullName == "System.String")
            return GenerateSystemStringProxy(type);

        // Split nested type name into parts (handling + outside generic markers)
        var nestingParts = SplitNestingParts(type.Name);
        var sanitizedName = SanitizeTypeName(nestingParts[^1]);
        var nestingDepth = nestingParts.Length - 1;

        // Determine base class: use proxy of CLR base type if known, otherwise _.System.Object
        var hasKnownBase = type.BaseTypeName is not null && _knownProxyTypes.Contains(type.BaseTypeName);
        var baseClass = hasKnownBase
            ? GetFullyQualifiedProxyType(type.BaseTypeName!)
            : "global::_.System.Object";

        // Don't seal types that serve as bases for other proxies
        var isSealed = !_baseTypes.Contains(type.FullName);
        var sealedKeyword = isSealed ? "sealed " : "";
        var partialKeyword = _nestingContainerTypes.Contains(type.FullName) ? "partial " : "";

        // Generate the class body with standard indentation
        var body = new StringBuilder();
        body.AppendLine($"public {sealedKeyword}{partialKeyword}class {sanitizedName} : {baseClass}");
        body.AppendLine("{");
        var ctorAccess = isSealed ? "private" : "protected";
        body.AppendLine($"    {ctorAccess} {sanitizedName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");

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
                body.AppendLine();
                body.AppendLine($"    // Duplicate field skipped: {field.Name} ({field.TypeName})");
                continue;
            }

            body.AppendLine();
            EmitProperty(body, type, field);
        }

        // FromAddress factory — use 'new' keyword if base also has FromAddress
        var newKeyword = hasKnownBase ? "new " : "";
        body.AppendLine();
        body.AppendLine($"    public static {newKeyword}{sanitizedName} FromAddress(ulong address, DumpContext ctx)");
        body.AppendLine($"        => new {sanitizedName}(address, ctx);");

        // GetInstances
        body.AppendLine();
        body.AppendLine($"    public static {newKeyword}global::System.Collections.Generic.IEnumerable<{sanitizedName}> GetInstances(DumpContext ctx)");
        body.AppendLine("    {");
        body.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        body.AppendLine($"            yield return new {sanitizedName}(addr, ctx);");
        body.AppendLine("    }");

        // ToString override
        body.AppendLine();
        body.AppendLine($"    public override string ToString() => $\"{sanitizedName}@0x{{_objAddress:X}}\";");

        body.AppendLine("}");

        // Build the final output with preamble and optional nesting wrapper
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();

        var proxyNamespace = GetProxyNamespace(type.Namespace);
        sb.AppendLine($"namespace {proxyNamespace};");
        sb.AppendLine();

        if (nestingDepth > 0)
        {
            // Open outer nesting classes
            for (int i = 0; i < nestingDepth; i++)
            {
                var indent = new string(' ', i * 4);
                sb.AppendLine($"{indent}public partial class {SanitizeTypeName(nestingParts[i])}");
                sb.AppendLine($"{indent}{{");
            }

            // Indent and append class body
            var nestIndent = new string(' ', nestingDepth * 4);
            foreach (var line in body.ToString().TrimEnd('\n', '\r').Split('\n'))
            {
                var trimmed = line.TrimEnd('\r');
                if (string.IsNullOrEmpty(trimmed))
                    sb.AppendLine();
                else
                    sb.AppendLine(nestIndent + trimmed);
            }

            // Close outer nesting classes
            sb.AppendLine();
            for (int i = nestingDepth - 1; i >= 0; i--)
            {
                sb.AppendLine($"{new string(' ', i * 4)}}}");
            }
        }
        else
        {
            sb.Append(body);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generate a generic proxy class for a generic type definition.
    /// Uses the given specialization as a template for field layout.
    /// </summary>
    private string GenerateGenericProxy(TypeMetadata representative)
    {
        var typeArgs = representative.GenericTypeArguments;
        var arity = typeArgs.Count;

        // Build type parameter names and mapping from CLR type arg → param name
        var typeParamNames = arity == 1
            ? ["T"]
            : Enumerable.Range(1, arity).Select(i => $"T{i}").ToArray();

        var typeArgMap = new Dictionary<string, string>();
        for (int i = 0; i < arity; i++)
            typeArgMap[typeArgs[i]] = typeParamNames[i];

        var defName = representative.GenericDefinitionName!;
        var typeParamList = string.Join(", ", typeParamNames);
        var classNameWithParams = $"{defName}<{typeParamList}>";

        var baseClass = "global::_.System.Object";

        var body = new StringBuilder();
        body.AppendLine($"public sealed class {classNameWithParams} : {baseClass}");
        body.AppendLine("{");
        body.AppendLine($"    private {defName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");

        // Properties for each field, substituting type parameters where they match
        var usedNames = new HashSet<string> { defName, "_objAddress", "_ctx" };
        foreach (var field in representative.Fields)
        {
            var propName = SanitizePropertyName(field.Name);
            if (!usedNames.Add(propName))
            {
                body.AppendLine();
                body.AppendLine($"    // Duplicate field skipped: {field.Name} ({field.TypeName})");
                continue;
            }

            body.AppendLine();
            EmitGenericProperty(body, field, typeArgMap);
        }

        // FromAddress factory
        body.AppendLine();
        body.AppendLine($"    public static new {classNameWithParams} FromAddress(ulong address, DumpContext ctx)");
        body.AppendLine($"        => new {classNameWithParams}(address, ctx);");

        // ToString override
        body.AppendLine();
        body.AppendLine($"    public override string ToString() => $\"{defName}@0x{{_objAddress:X}}\";");

        body.AppendLine("}");

        // Build the final output
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();

        var proxyNamespace = GetProxyNamespace(representative.Namespace);
        sb.AppendLine($"namespace {proxyNamespace};");
        sb.AppendLine();
        sb.Append(body);

        return sb.ToString();
    }

    private void EmitGenericProperty(StringBuilder sb, FieldInfo field, Dictionary<string, string> typeArgMap)
    {
        var propName = SanitizePropertyName(field.Name);

        // Check if the field's type matches a type argument
        switch (field.Kind)
        {
            case FieldKind.String:
                // Check if System.String is itself a type arg
                if (typeArgMap.TryGetValue("System.String", out var strParam))
                {
                    if (propName == field.Name)
                        sb.AppendLine($"    public {strParam} {propName} => Field<{strParam}>();");
                    else
                        sb.AppendLine($"    public {strParam} {propName} => Field<{strParam}>(\"{field.Name}\");");
                }
                else
                {
                    if (propName == field.Name)
                        sb.AppendLine($"    public string? {propName} => Field<string>();");
                    else
                        sb.AppendLine($"    public string? {propName} => Field<string>(\"{field.Name}\");");
                }
                break;

            case FieldKind.Primitive:
                var csType = field.TypeName;
                if (propName == field.Name)
                    sb.AppendLine($"    public {csType} {propName} => Field<{csType}>();");
                else
                    sb.AppendLine($"    public {csType} {propName} => Field<{csType}>(\"{field.Name}\");");
                break;

            case FieldKind.ObjectReference:
                if (field.ReferenceTypeName is not null && typeArgMap.TryGetValue(field.ReferenceTypeName, out var refParam))
                {
                    // Field type matches a type parameter
                    if (propName == field.Name)
                        sb.AppendLine($"    public {refParam}? {propName} => Field<{refParam}>();");
                    else
                        sb.AppendLine($"    public {refParam}? {propName} => Field<{refParam}>(\"{field.Name}\");");
                }
                else
                {
                    EmitObjectReferenceProperty(sb, null!, field, propName);
                }
                break;

            case FieldKind.Array:
                EmitGenericArrayProperty(sb, field, propName, typeArgMap);
                break;

            case FieldKind.ValueType:
                sb.AppendLine($"    // ValueType field: {field.Name} ({field.TypeName}) — not yet supported");
                break;

            default:
                sb.AppendLine($"    // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
    }

    private void EmitGenericArrayProperty(StringBuilder sb, FieldInfo field, string propName, Dictionary<string, string> typeArgMap)
    {
        var elementKind = field.ArrayElementKind ?? FieldKind.Unknown;
        var elementTypeName = field.ArrayElementTypeName;

        // Check if the element type matches a type argument
        if (elementTypeName is not null && typeArgMap.TryGetValue(elementTypeName, out var elemParam))
        {
            // Generic array element — use ReadArrayElement<T>
            var arrayAddrExpr = propName == field.Name ? "RefAddress()" : $"RefAddress(\"{field.Name}\")";
            sb.AppendLine($"    public global::Ndump.Core.DumpArray<{elemParam}>? {propName}");
            sb.AppendLine("    {");
            sb.AppendLine("        get");
            sb.AppendLine("        {");
            sb.AppendLine($"            var addr = {arrayAddrExpr};");
            sb.AppendLine("            if (addr == 0) return null;");
            sb.AppendLine("            var len = _ctx.GetArrayLength(addr);");
            sb.AppendLine($"            return new global::Ndump.Core.DumpArray<{elemParam}>(addr, len, i => ReadArrayElement<{elemParam}>(addr, i));");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            return;
        }

        // Not a type argument — fall back to normal array emission
        EmitArrayProperty(sb, null!, field, propName);
    }

    /// <summary>
    /// Generate the root System.Object proxy that declares _objAddress, _ctx, and GetObjAddress().
    /// All other proxies ultimately inherit from this type.
    /// </summary>
    private static string GenerateSystemObjectProxy(TypeMetadata type)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine("namespace _.System;");
        sb.AppendLine();
        sb.AppendLine("public class Object");
        sb.AppendLine("{");
        sb.AppendLine("    protected readonly ulong _objAddress;");
        sb.AppendLine("    protected readonly DumpContext _ctx;");
        sb.AppendLine();
        sb.AppendLine("    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<global::System.Type, global::System.Func<ulong, DumpContext, object>?> _proxyFactories = new();");
        sb.AppendLine();
        sb.AppendLine("    protected Object(ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public ulong GetObjAddress() => _objAddress;");
        sb.AppendLine();
        sb.AppendLine("    protected T Field<T>([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        if (typeof(T) == typeof(string))");
        sb.AppendLine("            return (T)(object)_ctx.GetStringField(_objAddress, fieldName)!;");
        sb.AppendLine("        if (!typeof(T).IsValueType)");
        sb.AppendLine("        {");
        sb.AppendLine("            var addr = _ctx.GetObjectAddress(_objAddress, fieldName);");
        sb.AppendLine("            if (addr == 0) return default!;");
        sb.AppendLine("            return (T)CreateProxy(typeof(T), addr, _ctx);");
        sb.AppendLine("        }");
        sb.AppendLine("        return _ctx.GetFieldValue<T>(_objAddress, fieldName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected ulong RefAddress([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("        => _ctx.GetObjectAddress(_objAddress, fieldName);");
        sb.AppendLine();
        sb.AppendLine("    protected T ReadArrayElement<T>(ulong arrayAddr, int index)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (typeof(T) == typeof(string))");
        sb.AppendLine("            return (T)(object)_ctx.GetArrayElementString(arrayAddr, index)!;");
        sb.AppendLine("        if (!typeof(T).IsValueType)");
        sb.AppendLine("        {");
        sb.AppendLine("            var addr = _ctx.GetArrayElementAddress(arrayAddr, index);");
        sb.AppendLine("            if (addr == 0) return default!;");
        sb.AppendLine("            return (T)CreateProxy(typeof(T), addr, _ctx);");
        sb.AppendLine("        }");
        sb.AppendLine("        return _ctx.GetArrayElementValue<T>(arrayAddr, index);");
        sb.AppendLine("    }");

        sb.AppendLine();
        sb.AppendLine("    private static object CreateProxy(global::System.Type proxyType, ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        var factory = _proxyFactories.GetOrAdd(proxyType, static t =>");
        sb.AppendLine("        {");
        sb.AppendLine("            var method = t.GetMethod(\"FromAddress\", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);");
        sb.AppendLine("            if (method is null) return null;");
        sb.AppendLine("            return (global::System.Func<ulong, DumpContext, object>)((a, c) => method.Invoke(null, [a, c])!);");
        sb.AppendLine("        });");
        sb.AppendLine("        return factory?.Invoke(address, ctx) ?? throw new global::System.InvalidOperationException($\"No FromAddress factory on {proxyType}\");");
        sb.AppendLine("    }");
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

    /// <summary>
    /// Generate the System.String proxy with implicit conversion to string and ToString override.
    /// </summary>
    private string GenerateSystemStringProxy(TypeMetadata type)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();
        sb.AppendLine("namespace _.System;");
        sb.AppendLine();

        var isSealed = !_baseTypes.Contains(type.FullName);
        var sealedKeyword = isSealed ? "sealed " : "";

        sb.AppendLine($"public {sealedKeyword}class String : global::_.System.Object");
        sb.AppendLine("{");
        var ctorAccess = isSealed ? "private" : "protected";
        sb.AppendLine($"    {ctorAccess} String(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");
        sb.AppendLine();
        sb.AppendLine("    public string? Value => _ctx.GetStringValue(_objAddress);");
        sb.AppendLine();
        sb.AppendLine("    public static implicit operator string?(String? proxy) => proxy?._ctx.GetStringValue(proxy._objAddress);");
        sb.AppendLine();
        sb.AppendLine("    public override string ToString() => _ctx.GetStringValue(_objAddress) ?? $\"String@0x{_objAddress:X}\";");
        sb.AppendLine();
        sb.AppendLine("    public static new String FromAddress(ulong address, DumpContext ctx)");
        sb.AppendLine("        => new String(address, ctx);");
        sb.AppendLine();
        sb.AppendLine("    public static new global::System.Collections.Generic.IEnumerable<String> GetInstances(DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
        sb.AppendLine("            yield return new String(addr, ctx);");
        sb.AppendLine("    }");
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
                if (propName == field.Name)
                    sb.AppendLine($"    public string? {propName} => Field<string>();");
                else
                    sb.AppendLine($"    public string? {propName} => Field<string>(\"{field.Name}\");");
                break;

            case FieldKind.Primitive:
                var csType = field.TypeName;
                if (propName == field.Name)
                    sb.AppendLine($"    public {csType} {propName} => Field<{csType}>();");
                else
                    sb.AppendLine($"    public {csType} {propName} => Field<{csType}>(\"{field.Name}\");");
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

    private bool IsUsableProxyType(string? typeName)
    {
        if (typeName is null) return false;
        if (_knownProxyTypes.Contains(typeName)) return true;
        // Check if it's a generic specialization whose definition is known
        var genDef = ExtractGenericDefinitionFullName(typeName);
        return genDef is not null && _knownGenericDefinitions.Contains(genDef);
    }

    /// <summary>
    /// Extract the fully-qualified generic definition name from a CLR type name.
    /// E.g., "System.Collections.Generic.List&lt;System.String&gt;" → "System.Collections.Generic.List"
    /// Returns null for non-generic names or compiler-generated &lt;&gt; patterns.
    /// </summary>
    private static string? ExtractGenericDefinitionFullName(string fullTypeName)
    {
        // Find the leaf's generic < (same logic as TypeInspector.FindGenericAngleInFullName)
        int d = 0;
        int lastPlus = -1;
        for (int i = 0; i < fullTypeName.Length; i++)
        {
            if (fullTypeName[i] == '<') d++;
            else if (fullTypeName[i] == '>') d--;
            else if (fullTypeName[i] == '+' && d == 0) lastPlus = i;
        }

        var searchStart = lastPlus + 1;
        for (int i = searchStart; i < fullTypeName.Length; i++)
        {
            if (fullTypeName[i] == '<')
            {
                if (i == searchStart) return null; // compiler name
                if (i + 1 < fullTypeName.Length && fullTypeName[i + 1] == '>') return null; // <>
                return fullTypeName[..i];
            }
        }
        return null;
    }

    private void EmitObjectReferenceProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName)
    {
        if (IsUsableProxyType(field.ReferenceTypeName))
        {
            var qualifiedProxyType = GetFullyQualifiedProxyType(field.ReferenceTypeName!);
            if (propName == field.Name)
                sb.AppendLine($"    public {qualifiedProxyType}? {propName} => Field<{qualifiedProxyType}>();");
            else
                sb.AppendLine($"    public {qualifiedProxyType}? {propName} => Field<{qualifiedProxyType}>(\"{field.Name}\");");
        }
        else
        {
            // Unknown reference type — expose as address
            if (propName == field.Name)
                sb.AppendLine($"    public ulong {propName} => RefAddress();");
            else
                sb.AppendLine($"    public ulong {propName} => RefAddress(\"{field.Name}\");");
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
                if (IsUsableProxyType(elementTypeName))
                {
                    var qualifiedProxy = GetFullyQualifiedProxyType(elementTypeName!);
                    csElementType = $"{qualifiedProxy}?";
                    var isBaseType = _baseTypes.Contains(elementTypeName!);
                    if (isBaseType)
                        readerLambda = $"i => {{ var ea = _ctx.GetArrayElementAddress(addr, i); return ea == 0 ? null : global::_.ProxyResolver.Resolve(ea, _ctx) as {qualifiedProxy} ?? {qualifiedProxy}.FromAddress(ea, _ctx); }}";
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

        var arrayAddrExpr = propName == field.Name ? "RefAddress()" : $"RefAddress(\"{field.Name}\")";
        sb.AppendLine($"    public global::Ndump.Core.DumpArray<{csElementType}>? {propName}");
        sb.AppendLine("    {");
        sb.AppendLine("        get");
        sb.AppendLine("        {");
        sb.AppendLine($"            var addr = {arrayAddrExpr};");
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
        // For generic types, use backtick notation for CLR reflection
        // E.g., "Dictionary<System.String, System.Object>" → "_.System.Collections.Generic.Dictionary`2"
        if (type.IsGenericInstance)
        {
            var ns = GetProxyNamespace(type.Namespace);
            return $"{ns}.{type.GenericDefinitionName}`{type.GenericTypeArguments.Count}";
        }

        // Split on nesting separators (+) outside generics, sanitize each part,
        // and rejoin with + (CLR nested type separator for Assembly.GetType)
        var parts = SplitNestingParts(type.Name);
        var sanitized = string.Join("+", parts.Select(SanitizeTypeName));
        return $"{GetProxyNamespace(type.Namespace)}.{sanitized}";
    }

    /// <summary>
    /// Get the fully qualified proxy type name for a given CLR full type name.
    /// E.g., "MyApp.Customer" → "_.MyApp.Customer"
    /// For generic types with known definitions: "System.Collections.Generic.List&lt;System.String&gt;"
    /// → "_.System.Collections.Generic.List&lt;string&gt;"
    /// </summary>
    private string GetFullyQualifiedProxyType(string fullTypeName)
    {
        // Check if this is a generic type with a known generic definition
        var genDef = ExtractGenericDefinitionFullName(fullTypeName);
        if (genDef is not null && _knownGenericDefinitions.Contains(genDef))
        {
            return GetFullyQualifiedGenericProxyType(fullTypeName, genDef);
        }

        return GetFullyQualifiedNonGenericProxyType(fullTypeName);
    }

    private static string GetFullyQualifiedNonGenericProxyType(string fullTypeName)
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
            return $"_.{ns}.{SanitizeTypeNameForSource(name)}";
        }
        return $"_.{SanitizeTypeNameForSource(fullTypeName)}";
    }

    private string GetFullyQualifiedGenericProxyType(string fullTypeName, string genDef)
    {
        // Parse the type arguments
        var (_, typeArgs) = TypeInspector.ParseGenericName(fullTypeName);
        // Extract the generic def short name (after last dot)
        var lastDot = genDef.LastIndexOf('.');
        var ns = lastDot > 0 ? genDef[..lastDot] : "";
        var defName = lastDot > 0 ? genDef[(lastDot + 1)..] : genDef;
        var proxyNs = GetProxyNamespace(ns);

        // Map each type argument to its proxy representation
        var mappedArgs = typeArgs.Select(MapClrTypeToProxyTypeArg);
        return $"{proxyNs}.{defName}<{string.Join(", ", mappedArgs)}>";
    }

    /// <summary>
    /// Map a CLR type argument name to the corresponding C# type for use as a generic argument.
    /// </summary>
    private string MapClrTypeToProxyTypeArg(string clrTypeName)
    {
        // Check for string
        if (clrTypeName == "System.String") return "string";

        // Check for known primitive types
        var primitive = MapClrPrimitiveTypeName(clrTypeName);
        if (primitive != "int" || clrTypeName == "System.Int32") return primitive;

        // Check if it's a known proxy type (non-generic)
        if (_knownProxyTypes.Contains(clrTypeName))
            return GetFullyQualifiedNonGenericProxyType(clrTypeName);

        // Check if it's a generic specialization with a known definition (recursive)
        var genDef = ExtractGenericDefinitionFullName(clrTypeName);
        if (genDef is not null && _knownGenericDefinitions.Contains(genDef))
            return GetFullyQualifiedGenericProxyType(clrTypeName, genDef);

        // If the name has no dot, it's likely an open generic parameter (e.g., "T1", "TKey")
        // from ClrMD reporting unresolved type params. Fall back to object.
        if (!clrTypeName.Contains('.'))
            return "object";

        // Fallback: use the real CLR type with global prefix
        return $"global::{clrTypeName}";
    }

    /// <summary>
    /// Sanitize a type name for use in C# source code, replacing nested type
    /// separators (+) outside generics with dots (.) for proper nested type access.
    /// </summary>
    private static string SanitizeTypeNameForSource(string name)
    {
        var parts = SplitNestingParts(name);
        return string.Join(".", parts.Select(SanitizeTypeName));
    }

    internal static string SanitizeTypeName(string name)
    {
        // Handle generic type names and other special characters.
        // Note: + (nested type separator) is intentionally kept — nesting
        // is handled at a higher level via SplitNestingParts.
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

    /// <summary>
    /// Split a type name on the nested type separator (+), but only at the top level
    /// (not inside generic angle brackets). E.g.:
    /// "RuntimeType+ActivatorCache" → ["RuntimeType", "ActivatorCache"]
    /// "Task+&lt;&gt;c" → ["Task", "&lt;&gt;c"]
    /// "Dictionary&lt;String, EventSource+Override&gt;" → ["Dictionary&lt;String, EventSource+Override&gt;"] (no split)
    /// </summary>
    internal static string[] SplitNestingParts(string name)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '<') depth++;
            else if (name[i] == '>') depth--;
            else if (name[i] == '+' && depth == 0)
            {
                parts.Add(name[start..i]);
                start = i + 1;
            }
        }
        parts.Add(name[start..]);
        return parts.ToArray();
    }

    internal static string GenerateProxyResolver()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace _;");
        sb.AppendLine();
        sb.AppendLine("internal static class ProxyResolver");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<string, global::System.Func<ulong, global::Ndump.Core.DumpContext, object>?> _cache = new();");
        sb.AppendLine();
        sb.AppendLine("    public static object? Resolve(ulong address, global::Ndump.Core.DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        var typeName = ctx.GetTypeName(address);");
        sb.AppendLine("        if (typeName is null) return null;");
        sb.AppendLine();
        sb.AppendLine("        var factory = _cache.GetOrAdd(typeName, static name =>");
        sb.AppendLine("        {");
        sb.AppendLine("            var proxyType = ResolveProxyType(name);");
        sb.AppendLine("            if (proxyType is null) return null;");
        sb.AppendLine();
        sb.AppendLine("            var method = proxyType.GetMethod(\"FromAddress\", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);");
        sb.AppendLine("            if (method is null) return null;");
        sb.AppendLine();
        sb.AppendLine("            return (global::System.Func<ulong, global::Ndump.Core.DumpContext, object>)((addr, c) => method.Invoke(null, [addr, c])!);");
        sb.AppendLine("        });");
        sb.AppendLine();
        sb.AppendLine("        return factory?.Invoke(address, ctx);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static global::System.Type? ResolveProxyType(string clrTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        var asm = typeof(ProxyResolver).Assembly;");
        sb.AppendLine();
        sb.AppendLine("        // Try non-generic lookup first");
        sb.AppendLine("        var proxyTypeName = MapToProxyTypeName(clrTypeName);");
        sb.AppendLine("        var proxyType = asm.GetType(proxyTypeName);");
        sb.AppendLine("        if (proxyType is not null) return proxyType;");
        sb.AppendLine();
        sb.AppendLine("        // Try generic lookup: parse type args and construct closed generic");
        sb.AppendLine("        var firstAngle = clrTypeName.IndexOf('<');");
        sb.AppendLine("        if (firstAngle < 0) return null;");
        sb.AppendLine();
        sb.AppendLine("        var typeArgs = ParseTypeArgs(clrTypeName, firstAngle);");
        sb.AppendLine("        var defName = clrTypeName[..firstAngle];");
        sb.AppendLine();
        sb.AppendLine("        // Find the generic definition using backtick notation");
        sb.AppendLine("        var lastDot = defName.LastIndexOf('.');");
        sb.AppendLine("        var ns = lastDot > 0 ? defName[..lastDot] : \"\";");
        sb.AppendLine("        var shortName = lastDot > 0 ? defName[(lastDot + 1)..] : defName;");
        sb.AppendLine("        var proxyNs = ns.Length > 0 ? \"_.\" + ns : \"_\";");
        sb.AppendLine("        var genDefTypeName = proxyNs + \".\" + shortName + \"`\" + typeArgs.Count;");
        sb.AppendLine();
        sb.AppendLine("        var genDefType = asm.GetType(genDefTypeName);");
        sb.AppendLine("        if (genDefType is null) return null;");
        sb.AppendLine();
        sb.AppendLine("        // Map each CLR type arg to its proxy type");
        sb.AppendLine("        var proxyArgTypes = new global::System.Type[typeArgs.Count];");
        sb.AppendLine("        for (int i = 0; i < typeArgs.Count; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            proxyArgTypes[i] = MapClrTypeArgToProxyType(typeArgs[i]);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        try { return genDefType.MakeGenericType(proxyArgTypes); }");
        sb.AppendLine("        catch { return null; }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static global::System.Type MapClrTypeArgToProxyType(string clrTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Primitives and well-known types");
        sb.AppendLine("        var t = clrTypeName switch");
        sb.AppendLine("        {");
        sb.AppendLine("            \"System.String\" => typeof(string),");
        sb.AppendLine("            \"System.Boolean\" => typeof(bool),");
        sb.AppendLine("            \"System.Byte\" => typeof(byte),");
        sb.AppendLine("            \"System.SByte\" => typeof(sbyte),");
        sb.AppendLine("            \"System.Int16\" => typeof(short),");
        sb.AppendLine("            \"System.UInt16\" => typeof(ushort),");
        sb.AppendLine("            \"System.Int32\" => typeof(int),");
        sb.AppendLine("            \"System.UInt32\" => typeof(uint),");
        sb.AppendLine("            \"System.Int64\" => typeof(long),");
        sb.AppendLine("            \"System.UInt64\" => typeof(ulong),");
        sb.AppendLine("            \"System.Single\" => typeof(float),");
        sb.AppendLine("            \"System.Double\" => typeof(double),");
        sb.AppendLine("            \"System.Char\" => typeof(char),");
        sb.AppendLine("            \"System.IntPtr\" => typeof(nint),");
        sb.AppendLine("            \"System.UIntPtr\" => typeof(nuint),");
        sb.AppendLine("            _ => null");
        sb.AppendLine("        };");
        sb.AppendLine("        if (t is not null) return t;");
        sb.AppendLine();
        sb.AppendLine("        // Try resolving as a proxy type (recursive for nested generics)");
        sb.AppendLine("        var proxyType = ResolveProxyType(clrTypeName);");
        sb.AppendLine("        if (proxyType is not null) return proxyType;");
        sb.AppendLine();
        sb.AppendLine("        // Fallback: try the real CLR type");
        sb.AppendLine("        return global::System.Type.GetType(clrTypeName) ?? typeof(object);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static global::System.Collections.Generic.List<string> ParseTypeArgs(string name, int firstAngle)");
        sb.AppendLine("    {");
        sb.AppendLine("        var args = new global::System.Collections.Generic.List<string>();");
        sb.AppendLine("        int depth = 0, start = firstAngle + 1;");
        sb.AppendLine("        for (int i = firstAngle; i < name.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (name[i] == '<') depth++;");
        sb.AppendLine("            else if (name[i] == '>')");
        sb.AppendLine("            {");
        sb.AppendLine("                depth--;");
        sb.AppendLine("                if (depth == 0) { args.Add(name[start..i].Trim()); break; }");
        sb.AppendLine("            }");
        sb.AppendLine("            else if (name[i] == ',' && depth == 1)");
        sb.AppendLine("            {");
        sb.AppendLine("                args.Add(name[start..i].Trim());");
        sb.AppendLine("                start = i + 1;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        return args;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string MapToProxyTypeName(string clrTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        var genericStart = clrTypeName.Length;");
        sb.AppendLine("        for (int i = 0; i < clrTypeName.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (clrTypeName[i] is '<' or '`' or '[')");
        sb.AppendLine("            {");
        sb.AppendLine("                genericStart = i;");
        sb.AppendLine("                break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var lastDot = clrTypeName.LastIndexOf('.', genericStart - 1);");
        sb.AppendLine("        if (lastDot > 0)");
        sb.AppendLine("        {");
        sb.AppendLine("            var ns = clrTypeName[..lastDot];");
        sb.AppendLine("            var name = clrTypeName[(lastDot + 1)..];");
        sb.AppendLine("            return \"_.\" + ns + \".\" + SanitizeNested(name);");
        sb.AppendLine("        }");
        sb.AppendLine("        return \"_.\" + SanitizeNested(clrTypeName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string SanitizeNested(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Split on + outside <>, sanitize each part, rejoin with + for CLR nested type lookup");
        sb.AppendLine("        var parts = new global::System.Collections.Generic.List<string>();");
        sb.AppendLine("        int depth = 0, start = 0;");
        sb.AppendLine("        for (int i = 0; i < name.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (name[i] == '<') depth++;");
        sb.AppendLine("            else if (name[i] == '>') depth--;");
        sb.AppendLine("            else if (name[i] == '+' && depth == 0)");
        sb.AppendLine("            {");
        sb.AppendLine("                parts.Add(Sanitize(name[start..i]));");
        sb.AppendLine("                start = i + 1;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        parts.Add(Sanitize(name[start..]));");
        sb.AppendLine("        return string.Join(\"+\", parts);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string Sanitize(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        return name");
        sb.AppendLine("            .Replace('<', '_').Replace('>', '_')");
        sb.AppendLine("            .Replace(',', '_').Replace(' ', '_')");
        sb.AppendLine("            .Replace('+', '_').Replace('.', '_')");
        sb.AppendLine("            .Replace('`', '_').Replace('[', '_')");
        sb.AppendLine("            .Replace(']', '_');");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
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
