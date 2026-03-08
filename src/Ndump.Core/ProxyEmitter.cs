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
    /// Maps backtick-form type names to concrete full names.
    /// E.g., "System.Collections.Generic.Dictionary`2+Entry" → "System.Collections.Generic.Dictionary&lt;System.String, System.Int32&gt;+Entry"
    /// Used when ClrMD reports array component types in backtick notation.
    /// </summary>
    private Dictionary<string, string> _backtickToFullName = [];

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

        var emittedNestedGeneric = new HashSet<string>();

        foreach (var type in types)
        {
            string code;
            string safeName;

            if (IsNestedInsideGenericParent(type))
            {
                // Emit in own file with partial generic outer class wrapper.
                // Deduplicate by backtick-form suffix (e.g., only one "Entry" across specializations).
                var bt = TypeNameHelper.ConvertToBacktickForm(type.FullName);
                if (!emittedNestedGeneric.Add(bt))
                    continue;
                code = GenerateNestedGenericTypeProxy(type);
                safeName = SanitizeTypeName(TypeNameHelper.ConvertToBacktickForm(type.Name));
            }
            else if (type.IsGenericInstance)
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

        // Build backtick-to-concrete mapping for value types nested inside generic parents.
        // ClrMD reports array component types using backtick notation (e.g., Dictionary`2+Entry)
        // but the discovered types use angle-bracket names (e.g., Dictionary<String, Int32>+Entry).
        _backtickToFullName = [];
        foreach (var type in types)
        {
            if (!type.FullName.Contains('<')) continue;
            var backtickForm = TypeNameHelper.ConvertToBacktickForm(type.FullName);
            if (backtickForm != type.FullName)
                _backtickToFullName.TryAdd(backtickForm, type.FullName);
        }
    }

    /// <summary>
    /// Convert a CLR type name with angle brackets to backtick arity form.
    /// E.g., "System.Collections.Generic.Dictionary&lt;System.String, System.Int32&gt;+Entry"
    /// → "System.Collections.Generic.Dictionary`2+Entry"
    /// </summary>
    private bool IsNestedInsideGenericParent(TypeMetadata type)
    {
        // Find the + that separates parent from nested name (outside angle brackets)
        int depth = 0, plusIdx = -1;
        for (int i = 0; i < type.FullName.Length; i++)
        {
            if (type.FullName[i] == '<') depth++;
            else if (type.FullName[i] == '>') depth--;
            else if (type.FullName[i] == '+' && depth == 0) { plusIdx = i; break; }
        }
        if (plusIdx < 0) return false;
        var parentFullName = type.FullName[..plusIdx];
        // Check if the parent is a generic type by looking for its definition in known generic definitions
        var parentGenDef = ExtractGenericDefinitionFullName(parentFullName);
        return parentGenDef is not null && _knownGenericDefinitions.Contains(parentGenDef);
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
        // Value types always extend _.System.Object directly (they use interior addressing, not CLR hierarchy)
        var isStructProxy = IsStructType(type);
        var hasKnownBase = !isStructProxy && type.BaseTypeName is not null && _knownProxyTypes.Contains(type.BaseTypeName);
        var baseClass = hasKnownBase
            ? GetFullyQualifiedProxyType(type.BaseTypeName!)
            : "global::_.System.Object";

        // Don't seal types that serve as bases for other proxies
        var isSealed = !_baseTypes.Contains(type.FullName);
        var sealedKeyword = isSealed ? "sealed " : "";
        var partialKeyword = _nestingContainerTypes.Contains(type.FullName) ? "partial " : "";

        // Generate the class body with standard indentation
        var body = new StringBuilder();
        var interfaces = isStructProxy ? $", global::Ndump.Core.IProxy<{sanitizedName}>" : "";
        body.AppendLine($"public {sealedKeyword}{partialKeyword}class {sanitizedName} : {baseClass}{interfaces}");
        body.AppendLine("{");
        var ctorAccess = isSealed ? "private" : "protected";
        body.AppendLine($"    {ctorAccess} {sanitizedName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");
        if (isStructProxy)
        {
            // Value type proxy: additional constructor for interior (embedded) struct fields and array elements
            body.AppendLine($"    {ctorAccess} {sanitizedName}(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) {{ }}");
        }

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
        // Value type proxies always need 'new' because they shadow _.System.Object's FromAddress
        var newKeyword = hasKnownBase || isStructProxy ? "new " : "";
        body.AppendLine();
        body.AppendLine($"    public static {newKeyword}{sanitizedName} FromAddress(ulong address, DumpContext ctx)");
        body.AppendLine($"        => new {sanitizedName}(address, ctx);");

        if (isStructProxy)
        {
            // Factory for interior (embedded) struct fields and array elements
            body.AppendLine();
            body.AppendLine($"    public static {sanitizedName} FromInterior(ulong address, DumpContext ctx, string interiorTypeName)");
            body.AppendLine($"        => new {sanitizedName}(address, ctx, interiorTypeName);");
        }

        if (!isStructProxy)
        {
            // GetInstances — only for reference types (heap objects)
            body.AppendLine();
            body.AppendLine($"    public static {newKeyword}global::System.Collections.Generic.IEnumerable<{sanitizedName}> GetInstances(DumpContext ctx)");
            body.AppendLine("    {");
            body.AppendLine($"        foreach (var addr in ctx.EnumerateInstances(\"{type.FullName}\"))");
            body.AppendLine($"            yield return new {sanitizedName}(addr, ctx);");
            body.AppendLine("    }");
        }

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

        // Check if any nested types exist for this generic definition
        var ownerBacktick = TypeNameHelper.ConvertToBacktickForm(representative.FullName);
        var hasNestedTypes = _typesByName.Values.Any(t =>
        {
            var bt = TypeNameHelper.ConvertToBacktickForm(t.FullName);
            var plusIdx = bt.IndexOf('+');
            return plusIdx > 0 && bt[..plusIdx] == ownerBacktick;
        });
        var classModifiers = hasNestedTypes ? "partial" : "sealed";

        var isStructProxy = IsStructType(representative);
        var body = new StringBuilder();
        var interfaces = isStructProxy ? $", global::Ndump.Core.IProxy<{classNameWithParams}>" : "";
        body.AppendLine($"public {classModifiers} class {classNameWithParams} : {baseClass}{interfaces}");
        body.AppendLine("{");
        body.AppendLine($"    private {defName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");
        if (isStructProxy)
        {
            body.AppendLine($"    private {defName}(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) {{ }}");
        }

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
            EmitGenericProperty(body, representative, field, typeArgMap);
        }

        // FromAddress factory
        body.AppendLine();
        body.AppendLine($"    public static new {classNameWithParams} FromAddress(ulong address, DumpContext ctx)");
        body.AppendLine($"        => new {classNameWithParams}(address, ctx);");

        if (isStructProxy)
        {
            body.AppendLine();
            body.AppendLine($"    public static {classNameWithParams} FromInterior(ulong address, DumpContext ctx, string interiorTypeName)");
            body.AppendLine($"        => new {classNameWithParams}(address, ctx, interiorTypeName);");
        }

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

    /// <summary>
    /// Generate a proxy for a type nested inside a generic parent (e.g., Dictionary+KeyCollection, Dictionary+Entry).
    /// Emits the nested class in its own file, wrapped in a partial generic outer class.
    /// </summary>
    private string GenerateNestedGenericTypeProxy(TypeMetadata nestedType)
    {
        // Find the + separator between parent and nested name (outside angle brackets)
        int depth = 0, origPlusIdx = -1;
        for (int i = 0; i < nestedType.FullName.Length; i++)
        {
            if (nestedType.FullName[i] == '<') depth++;
            else if (nestedType.FullName[i] == '>') depth--;
            else if (nestedType.FullName[i] == '+' && depth == 0) { origPlusIdx = i; break; }
        }
        if (origPlusIdx < 0)
            return $"// ERROR: No nesting separator found in {nestedType.FullName}";

        var parentFullName = nestedType.FullName[..origPlusIdx];
        var nestedSuffix = nestedType.FullName[(origPlusIdx + 1)..];
        var sanitizedNestedName = SanitizeTypeName(nestedSuffix);

        // _genericRepresentatives keys are GenericDefinitionFullName (e.g., "System.Collections.Generic.Dictionary")
        var parentGenDef = ExtractGenericDefinitionFullName(parentFullName);
        if (parentGenDef is null || !_genericRepresentatives.TryGetValue(parentGenDef, out var parentRep))
            return $"// ERROR: Could not find parent generic definition for {nestedType.FullName}";

        // Build type arg map from the nested type's own parent specialization, not the representative.
        // E.g., for Dictionary<String, Int32>+KeyCollection, use [String, Int32] not [String, Object].
        var (_, parentTypeArgs) = TypeInspector.ParseGenericName(parentFullName);
        var arity = parentTypeArgs.Count;
        var typeParamNames = arity == 1
            ? ["T"]
            : Enumerable.Range(1, arity).Select(i => $"T{i}").ToArray();
        var typeArgMap = new Dictionary<string, string>();
        for (int i = 0; i < arity; i++)
            typeArgMap[parentTypeArgs[i]] = typeParamNames[i];

        var parentDefName = parentRep.GenericDefinitionName!;
        var typeParamList = string.Join(", ", typeParamNames);

        // Build the nested class body
        var body = new StringBuilder();
        var nestedInterfaces = nestedType.IsValueType ? $", global::Ndump.Core.IProxy<{sanitizedNestedName}>" : "";
        body.AppendLine($"    public sealed class {sanitizedNestedName} : global::_.System.Object{nestedInterfaces}");
        body.AppendLine("    {");

        body.AppendLine($"        private {sanitizedNestedName}(ulong address, DumpContext ctx) : base(address, ctx) {{ }}");
        if (nestedType.IsValueType)
        {
            body.AppendLine($"        private {sanitizedNestedName}(ulong address, DumpContext ctx, string interiorTypeName) : base(address, ctx, interiorTypeName) {{ }}");
        }

        // Emit fields with type param substitution
        var usedNames = new HashSet<string> { sanitizedNestedName, "_objAddress", "_ctx" };
        foreach (var field in nestedType.Fields)
        {
            var propName = SanitizePropertyName(field.Name);
            if (!usedNames.Add(propName)) continue;

            body.AppendLine();

            // Check if this field's type matches a parent type arg (via __Canon or concrete types)
            if (field.Kind == FieldKind.ObjectReference && field.ReferenceTypeName is not null
                && typeArgMap.TryGetValue(field.ReferenceTypeName, out var typeParam))
            {
                EmitNestedFieldWithTypeParam(body, field, propName, typeParam);
            }
            else if (field.Kind == FieldKind.ObjectReference && field.ReferenceTypeName == "System.__Canon")
            {
                var canonParam = ResolveCanonTypeParam(nestedType, field, typeArgMap);
                if (canonParam is not null)
                    EmitNestedFieldWithTypeParam(body, field, propName, canonParam);
                else
                    EmitNestedField(body, nestedType, field, propName, typeArgMap);
            }
            else
            {
                EmitNestedField(body, nestedType, field, propName, typeArgMap);
            }
        }

        body.AppendLine();
        body.AppendLine($"        public static new {sanitizedNestedName} FromAddress(ulong address, DumpContext ctx)");
        body.AppendLine($"            => new {sanitizedNestedName}(address, ctx);");

        if (nestedType.IsValueType)
        {
            body.AppendLine();
            body.AppendLine($"        public static {sanitizedNestedName} FromInterior(ulong address, DumpContext ctx, string interiorTypeName)");
            body.AppendLine($"            => new {sanitizedNestedName}(address, ctx, interiorTypeName);");
        }

        body.AppendLine();
        body.AppendLine($"        public override string ToString() => $\"{sanitizedNestedName}@0x{{_objAddress:X}}\";");
        body.AppendLine("    }");

        // Build the final output: partial outer class wrapper
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Ndump.Core;");
        sb.AppendLine();

        var proxyNamespace = GetProxyNamespace(parentRep.Namespace);
        sb.AppendLine($"namespace {proxyNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {parentDefName}<{typeParamList}>");
        sb.AppendLine("{");
        sb.Append(body);
        sb.AppendLine("}");

        return sb.ToString();
    }

    private void EmitGenericProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, Dictionary<string, string> typeArgMap)
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
                    EmitGenericObjectReferenceProperty(sb, ownerType, field, propName, typeArgMap);
                }
                break;

            case FieldKind.Array:
                EmitGenericArrayProperty(sb, ownerType, field, propName, typeArgMap);
                break;

            case FieldKind.ValueType:
                EmitValueTypeProperty(sb, field, propName);
                break;

            default:
                sb.AppendLine($"    // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
    }

    private static void EmitNestedFieldWithTypeParam(StringBuilder sb, FieldInfo field, string propName, string typeParam)
    {
        if (propName == field.Name)
            sb.AppendLine($"        public {typeParam}? {propName} => Field<{typeParam}>();");
        else
            sb.AppendLine($"        public {typeParam}? {propName} => Field<{typeParam}>(\"{field.Name}\");");
    }

    /// <summary>
    /// Emit a field property inside a nested type of a generic proxy.
    /// Handles __Canon and backtick references by resolving them relative to the parent generic.
    /// </summary>
    private void EmitNestedField(StringBuilder sb, TypeMetadata nestedType, FieldInfo field, string propName, Dictionary<string, string> typeArgMap)
    {
        if (field.Kind == FieldKind.ObjectReference && field.ReferenceTypeName is not null)
        {
            // Replace __Canon with type params and concrete type args with type params
            var resolved = ResolveNestedFieldType(field.ReferenceTypeName, typeArgMap);
            if (resolved is not null)
            {
                if (propName == field.Name)
                    sb.AppendLine($"        public {resolved}? {propName} => Field<{resolved}>();");
                else
                    sb.AppendLine($"        public {resolved}? {propName} => Field<{resolved}>(\"{field.Name}\");");
                return;
            }
        }

        EmitNestedFieldSimple(sb, field, propName);
    }

    /// <summary>
    /// Resolve an object reference type name inside a nested generic type.
    /// Replaces __Canon with type params, and maps concrete type args back to type params.
    /// Returns the C# type expression (e.g., "Dictionary&lt;T1, T2&gt;") or null if unresolvable.
    /// </summary>
    private string? ResolveNestedFieldType(string refTypeName, Dictionary<string, string> typeArgMap)
    {
        // First replace __Canon with type params
        var typeName = refTypeName.Contains("__Canon")
            ? ReplaceCanonWithTypeParams(refTypeName, typeArgMap)
            : refTypeName;

        // Also replace concrete type args with type params (e.g., System.String → T1)
        foreach (var (concreteArg, typeParam) in typeArgMap)
            typeName = typeName.Replace(concreteArg, typeParam);

        // Check if the resolved type is a known generic definition
        var genDef = ExtractGenericDefinitionFullName(typeName);
        if (genDef is not null && _knownGenericDefinitions.Contains(genDef))
        {
            // Build the proxy type reference with type params preserved
            var lastDot = genDef.LastIndexOf('.');
            var ns = lastDot > 0 ? genDef[..lastDot] : "";
            var defName = lastDot > 0 ? genDef[(lastDot + 1)..] : genDef;
            var proxyNs = GetProxyNamespace(ns);

            // Extract and map type arguments, keeping type params as-is
            var (_, typeArgs) = TypeInspector.ParseGenericName(typeName);
            var mappedArgs = typeArgs.Select(arg =>
            {
                // If it's a type param (no dot, like "T1"), keep it
                if (!arg.Contains('.')) return arg;
                // Otherwise map CLR type to C# type
                return MapClrTypeToProxyTypeArg(arg);
            });
            return $"{proxyNs}.{defName}<{string.Join(", ", mappedArgs)}>";
        }

        // Check if it's a known non-generic proxy type
        if (_knownProxyTypes.Contains(typeName))
            return GetFullyQualifiedNonGenericProxyType(typeName);

        return null;
    }

    private static void EmitNestedFieldSimple(StringBuilder sb, FieldInfo field, string propName)
    {
        switch (field.Kind)
        {
            case FieldKind.Primitive:
                if (propName == field.Name)
                    sb.AppendLine($"        public {field.TypeName} {propName} => Field<{field.TypeName}>();");
                else
                    sb.AppendLine($"        public {field.TypeName} {propName} => Field<{field.TypeName}>(\"{field.Name}\");");
                break;

            case FieldKind.String:
                if (propName == field.Name)
                    sb.AppendLine($"        public string? {propName} => Field<string>();");
                else
                    sb.AppendLine($"        public string? {propName} => Field<string>(\"{field.Name}\");");
                break;

            case FieldKind.ObjectReference:
                // Object reference in nested type — map to _.System.Object
                if (propName == field.Name)
                    sb.AppendLine($"        public global::_.System.Object? {propName} => Field<global::_.System.Object>();");
                else
                    sb.AppendLine($"        public global::_.System.Object? {propName} => Field<global::_.System.Object>(\"{field.Name}\");");
                break;

            default:
                sb.AppendLine($"        // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
    }

    /// <summary>
    /// For a __Canon field on a nested value type, determine which parent type param it corresponds to.
    /// Uses positional matching: first __Canon → first ref-type param, second __Canon → second ref-type param.
    /// </summary>
    private static string? ResolveCanonTypeParam(TypeMetadata vt, FieldInfo targetField, Dictionary<string, string> typeArgMap)
    {
        // Collect all type params that map to reference types (i.e., would appear as __Canon)
        var refTypeParams = typeArgMap
            .Where(kv => kv.Key != "System.String" || kv.Key.Contains('.'))
            .Select(kv => kv.Value)
            .ToList();

        // Actually, a simpler approach: count __Canon fields and map by order to the parent's type params
        // This works because CLR lays out shared generic fields in order of the type params
        int canonIndex = 0;
        var typeParamValues = typeArgMap.Values.ToList();
        foreach (var f in vt.Fields)
        {
            if (f.Kind == FieldKind.ObjectReference && f.ReferenceTypeName == "System.__Canon")
            {
                if (f == targetField)
                    return canonIndex < typeParamValues.Count ? typeParamValues[canonIndex] : null;
                canonIndex++;
            }
        }
        return null;
    }

    /// <summary>
    /// Replace __Canon type arguments inside a generic type name with the parent's type parameters.
    /// E.g., "System.Collections.Generic.Dictionary&lt;System.__Canon, System.Int32&gt;"
    /// with typeArgMap {System.String→T1, System.Int32→T2}
    /// → "System.Collections.Generic.Dictionary&lt;T1, System.Int32&gt;"
    /// Uses positional matching: first __Canon → first ref-type param, etc.
    /// </summary>
    private static string ReplaceCanonWithTypeParams(string typeName, Dictionary<string, string> typeArgMap)
    {
        // Simple approach: replace each __Canon occurrence with the next type param
        var typeParams = typeArgMap.Values.ToList();
        int canonIndex = 0;
        var result = new StringBuilder();
        int i = 0;
        const string canon = "System.__Canon";
        while (i < typeName.Length)
        {
            if (i + canon.Length <= typeName.Length && typeName[i..(i + canon.Length)] == canon)
            {
                if (canonIndex < typeParams.Count)
                    result.Append(typeParams[canonIndex++]);
                else
                    result.Append(canon);
                i += canon.Length;
            }
            else
            {
                result.Append(typeName[i]);
                i++;
            }
        }
        return result.ToString();
    }

    private void EmitGenericArrayProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName, Dictionary<string, string> typeArgMap)
    {
        var elementKind = field.ArrayElementKind ?? FieldKind.Unknown;
        var elementTypeName = field.ArrayElementTypeName;

        // Check if the element type matches a type argument
        if (elementTypeName is not null && typeArgMap.TryGetValue(elementTypeName, out var elemParam))
        {
            var fieldNameArg = propName == field.Name ? "" : $"\"{field.Name}\"";
            sb.AppendLine($"    public global::Ndump.Core.DumpArray<{elemParam}>? {propName} => ArrayField<{elemParam}>({fieldNameArg});");
            return;
        }

        // Check if this is a value-type array element nested inside this generic parent
        if (elementKind == FieldKind.ValueType && elementTypeName is not null && elementTypeName.Contains('`'))
        {
            var plusIdx = elementTypeName.IndexOf('+');
            if (plusIdx > 0)
            {
                var backtickPrefix = elementTypeName[..plusIdx];
                var ownerBacktick = TypeNameHelper.ConvertToBacktickForm(ownerType.FullName);
                if (ownerBacktick == backtickPrefix)
                {
                    var nestedSuffix = elementTypeName[(plusIdx + 1)..];
                    var sanitizedNested = SanitizeTypeName(nestedSuffix);
                    var fieldNameArg = propName == field.Name ? "" : $"\"{field.Name}\"";
                    sb.AppendLine($"    public global::Ndump.Core.DumpArray<{sanitizedNested}>? {propName} => StructArrayField<{sanitizedNested}>({fieldNameArg});");
                    return;
                }
            }
        }

        // Not a type argument — fall back to normal array emission (resolves backtick names via ownerType)
        EmitArrayProperty(sb, ownerType, field, propName);
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
        sb.AppendLine("    // For interior struct fields: the CLR type name of this struct");
        sb.AppendLine("    protected readonly string? _interiorTypeName;");
        sb.AppendLine();
        sb.AppendLine("    protected Object(ulong address, DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected Object(ulong address, DumpContext ctx, string interiorTypeName)");
        sb.AppendLine("    {");
        sb.AppendLine("        _objAddress = address;");
        sb.AppendLine("        _ctx = ctx;");
        sb.AppendLine("        _interiorTypeName = interiorTypeName;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public ulong GetObjAddress() => _objAddress;");
        sb.AppendLine();
        sb.AppendLine("    protected T Field<T>([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_interiorTypeName is not null)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (typeof(T) == typeof(string))");
        sb.AppendLine("                return (T)(object)_ctx.GetStringField(_objAddress, _interiorTypeName, fieldName)!;");
        sb.AppendLine("            if (!typeof(T).IsValueType)");
        sb.AppendLine("            {");
        sb.AppendLine("                var addr = _ctx.GetObjectAddress(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("                if (addr == 0) return default!;");
        sb.AppendLine("                return global::_.ProxyResolver.Resolve<T>(addr, _ctx);");
        sb.AppendLine("            }");
        sb.AppendLine("            return _ctx.GetFieldValue<T>(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("        }");
        sb.AppendLine("        if (typeof(T) == typeof(string))");
        sb.AppendLine("            return (T)(object)_ctx.GetStringField(_objAddress, fieldName)!;");
        sb.AppendLine("        if (!typeof(T).IsValueType)");
        sb.AppendLine("        {");
        sb.AppendLine("            var addr = _ctx.GetObjectAddress(_objAddress, fieldName);");
        sb.AppendLine("            if (addr == 0) return default!;");
        sb.AppendLine("            return global::_.ProxyResolver.Resolve<T>(addr, _ctx);");
        sb.AppendLine("        }");
        sb.AppendLine("        return _ctx.GetFieldValue<T>(_objAddress, fieldName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected T StructField<T>(string structTypeName, [CallerMemberName] string fieldName = \"\") where T : global::Ndump.Core.IProxy<T>");
        sb.AppendLine("    {");
        sb.AppendLine("        ulong addr;");
        sb.AppendLine("        if (_interiorTypeName is not null)");
        sb.AppendLine("            addr = _ctx.GetInteriorValueTypeFieldAddress(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("        else");
        sb.AppendLine("            addr = _ctx.GetValueTypeFieldAddress(_objAddress, fieldName);");
        sb.AppendLine("        return T.FromInterior(addr, _ctx, structTypeName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected T? NullableField<T>([CallerMemberName] string fieldName = \"\") where T : struct");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_interiorTypeName is not null)");
        sb.AppendLine("            return _ctx.GetNullableFieldValue<T>(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("        return _ctx.GetNullableFieldValue<T>(_objAddress, fieldName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected T? NullableStructField<T>(string innerTypeName, [CallerMemberName] string fieldName = \"\") where T : class, global::Ndump.Core.IProxy<T>");
        sb.AppendLine("    {");
        sb.AppendLine("        (bool hasValue, ulong valueAddr) info;");
        sb.AppendLine("        if (_interiorTypeName is not null)");
        sb.AppendLine("            info = _ctx.GetNullableFieldInfo(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("        else");
        sb.AppendLine("            info = _ctx.GetNullableFieldInfo(_objAddress, fieldName);");
        sb.AppendLine("        if (!info.hasValue) return null;");
        sb.AppendLine("        return T.FromInterior(info.valueAddr, _ctx, innerTypeName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected ulong RawFieldAddress([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_interiorTypeName is not null)");
        sb.AppendLine("            return _ctx.GetInteriorValueTypeFieldAddress(_objAddress, _interiorTypeName, fieldName);");
        sb.AppendLine("        return _ctx.GetValueTypeFieldAddress(_objAddress, fieldName);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected ulong RefAddress([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("        => _interiorTypeName is not null");
        sb.AppendLine("            ? _ctx.GetObjectAddress(_objAddress, _interiorTypeName, fieldName)");
        sb.AppendLine("            : _ctx.GetObjectAddress(_objAddress, fieldName);");
        sb.AppendLine();
        sb.AppendLine("    protected global::Ndump.Core.DumpArray<T>? ArrayField<T>([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        var addr = RefAddress(fieldName);");
        sb.AppendLine("        if (addr == 0) return null;");
        sb.AppendLine("        var len = _ctx.GetArrayLength(addr);");
        sb.AppendLine("        return new global::Ndump.Core.DumpArray<T>(addr, len, i => ReadArrayElement<T>(addr, i));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected global::Ndump.Core.DumpArray<ulong>? ArrayAddresses([CallerMemberName] string fieldName = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        var addr = RefAddress(fieldName);");
        sb.AppendLine("        if (addr == 0) return null;");
        sb.AppendLine("        var len = _ctx.GetArrayLength(addr);");
        sb.AppendLine("        return new global::Ndump.Core.DumpArray<ulong>(addr, len, i => _ctx.GetArrayElementAddress(addr, i));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected T ReadArrayElement<T>(ulong arrayAddr, int index)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (typeof(T) == typeof(string))");
        sb.AppendLine("            return (T)(object)_ctx.GetArrayElementString(arrayAddr, index)!;");
        sb.AppendLine("        if (!typeof(T).IsValueType)");
        sb.AppendLine("        {");
        sb.AppendLine("            var addr = _ctx.GetArrayElementAddress(arrayAddr, index);");
        sb.AppendLine("            if (addr == 0) return default!;");
        sb.AppendLine("            return global::_.ProxyResolver.Resolve<T>(addr, _ctx);");
        sb.AppendLine("        }");
        sb.AppendLine("        return _ctx.GetArrayElementValue<T>(arrayAddr, index);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected global::Ndump.Core.DumpArray<T>? StructArrayField<T>([CallerMemberName] string fieldName = \"\") where T : global::Ndump.Core.IProxy<T>");
        sb.AppendLine("    {");
        sb.AppendLine("        var addr = RefAddress(fieldName);");
        sb.AppendLine("        if (addr == 0) return null;");
        sb.AppendLine("        var len = _ctx.GetArrayLength(addr);");
        sb.AppendLine("        var typeName = _ctx.GetArrayComponentTypeName(addr);");
        sb.AppendLine("        return new global::Ndump.Core.DumpArray<T>(addr, len, i =>");
        sb.AppendLine("        {");
        sb.AppendLine("            var ea = _ctx.GetArrayStructElementAddress(addr, i);");
        sb.AppendLine("            return T.FromInterior(ea, _ctx, typeName);");
        sb.AppendLine("        });");
        sb.AppendLine("    }");



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
                EmitValueTypeProperty(sb, field, propName);
                break;

            default:
                sb.AppendLine($"    // Unknown field: {field.Name} ({field.TypeName})");
                break;
        }
    }

    /// <summary>
    /// True if the type is a struct proxy — either explicitly marked as a value type,
    /// or has a base type of System.ValueType or System.Enum (for types discovered via heap walk).
    /// </summary>
    private static bool IsStructType(TypeMetadata type) =>
        type.IsValueType || type.BaseTypeName is "System.ValueType" or "System.Enum";

    private static bool IsUselessValueType(string? typeName) => typeName is
        null or "System.ValueType";

    /// <summary>
    /// True if the type name is System.Void — a type that ClrMD reports when it cannot resolve
    /// the actual type of a value type field (e.g., delegates, types never instantiated on the heap).
    /// </summary>
    private static bool IsVoidType(string? typeName) => typeName is "System.Void";

    /// <summary>
    /// Check if a value type name has unresolved generic type parameters (e.g., "System.Nullable&lt;T1&gt;").
    /// ClrMD reports these when it can't resolve the concrete specialization.
    /// </summary>
    private static bool HasUnresolvedTypeParams(string typeName)
    {
        var angleIdx = typeName.IndexOf('<');
        if (angleIdx < 0) return false;
        // Parse type args and check if any look like open params (no dots, short names like T, T1, TKey)
        var (_, args) = TypeInspector.ParseGenericName(typeName);
        return args.Any(a => !a.Contains('.'));
    }

    /// <summary>
    /// Map a resolved Nullable inner type name to the C# primitive keyword.
    /// Returns null if the type is not a primitive (e.g., DateTime is a struct, not a primitive).
    /// </summary>
    private static string? MapNullableInnerToCs(string innerTypeName)
    {
        return innerTypeName switch
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
            _ => null
        };
    }

    /// <summary>
    /// Check if a Nullable inner type name is a primitive (can be read with NullableField&lt;T&gt;).
    /// </summary>
    private static bool IsNullablePrimitive(string innerTypeName)
        => MapNullableInnerToCs(innerTypeName) is not null;

    private void EmitValueTypeProperty(StringBuilder sb, FieldInfo field, string propName)
    {
        // Handle Nullable<T> with a resolved inner type
        if (field.IsNullableValueType)
        {
            EmitNullableProperty(sb, field, propName);
            return;
        }

        // Handle System.Void — expose raw interior address
        if (IsVoidType(field.ReferenceTypeName))
        {
            if (propName == field.Name)
                sb.AppendLine($"    public ulong {propName} => RawFieldAddress();");
            else
                sb.AppendLine($"    public ulong {propName} => RawFieldAddress(\"{field.Name}\");");
            return;
        }

        if (!IsUselessValueType(field.ReferenceTypeName)
            && !HasUnresolvedTypeParams(field.ReferenceTypeName!)
            && IsUsableProxyType(field.ReferenceTypeName))
        {
            var qualifiedProxyType = GetFullyQualifiedProxyType(field.ReferenceTypeName!);
            if (propName == field.Name)
                sb.AppendLine($"    public {qualifiedProxyType} {propName} => StructField<{qualifiedProxyType}>(\"{field.ReferenceTypeName}\");");
            else
                sb.AppendLine($"    public {qualifiedProxyType} {propName} => StructField<{qualifiedProxyType}>(\"{field.ReferenceTypeName}\", \"{field.Name}\");");
        }
        else
        {
            sb.AppendLine($"    // ValueType field: {field.Name} ({field.ReferenceTypeName ?? field.TypeName}) — no proxy available");
        }
    }

    private void EmitNullableProperty(StringBuilder sb, FieldInfo field, string propName)
    {
        var innerType = field.NullableInnerTypeName!;
        var csType = MapNullableInnerToCs(innerType);

        if (csType is not null)
        {
            // Nullable primitive: emit as T? using NullableField<T>()
            if (propName == field.Name)
                sb.AppendLine($"    public {csType}? {propName} => NullableField<{csType}>();");
            else
                sb.AppendLine($"    public {csType}? {propName} => NullableField<{csType}>(\"{field.Name}\");");
        }
        else if (IsUsableProxyType(innerType))
        {
            // Nullable complex struct with known proxy: emit with NullableStructField
            var qualifiedProxyType = GetFullyQualifiedProxyType(innerType);
            if (propName == field.Name)
                sb.AppendLine($"    public {qualifiedProxyType}? {propName} => NullableStructField<{qualifiedProxyType}>(\"{innerType}\");");
            else
                sb.AppendLine($"    public {qualifiedProxyType}? {propName} => NullableStructField<{qualifiedProxyType}>(\"{innerType}\", \"{field.Name}\");");
        }
        else
        {
            sb.AppendLine($"    // Nullable<{innerType}> field: {field.Name} — inner type has no proxy available");
        }
    }

    private bool IsUsableProxyType(string? typeName)
    {
        if (typeName is null) return false;
        if (_knownProxyTypes.Contains(typeName)) return true;
        // Check backtick-form mapping (e.g., Dictionary`2+Entry → Dictionary<String, Int32>+Entry)
        if (_backtickToFullName.ContainsKey(typeName)) return true;
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
            // Unknown reference type — expose as _.System.Object
            if (propName == field.Name)
                sb.AppendLine($"    public global::_.System.Object? {propName} => Field<global::_.System.Object>();");
            else
                sb.AppendLine($"    public global::_.System.Object? {propName} => Field<global::_.System.Object>(\"{field.Name}\");");
        }
    }

    /// <summary>
    /// Emit an object reference property inside a generic proxy, handling:
    /// 1. Nested types of the same generic parent (e.g., KeyCollection → just "KeyCollection")
    /// 2. __Canon references that need substitution with type params
    /// Falls back to the standard EmitObjectReferenceProperty for other cases.
    /// </summary>
    /// <summary>
    /// Check if a field's reference type is nested inside the same generic parent,
    /// handling all CLR name forms: concrete (Dictionary&lt;String,Int32&gt;+KeyCollection),
    /// backtick (Dictionary`2+KeyCollection), and __Canon (Dictionary&lt;__Canon,Int32&gt;+KeyCollection).
    /// Returns the nested suffix (e.g., "KeyCollection") or null if not nested.
    /// </summary>
    private string? TryGetNestedSuffix(string refTypeName, TypeMetadata ownerType)
    {
        // Check concrete form: "Dictionary<String, Object>+KeyCollection"
        var prefix = ownerType.FullName + "+";
        if (refTypeName.StartsWith(prefix))
            return refTypeName[prefix.Length..];

        // Convert both to backtick form and compare the parent portion
        var refBacktick = TypeNameHelper.ConvertToBacktickForm(refTypeName);
        var plusIdx = refBacktick.IndexOf('+');
        if (plusIdx <= 0) return null;

        var refParent = refBacktick[..plusIdx];
        var ownerBacktick = TypeNameHelper.ConvertToBacktickForm(ownerType.FullName);
        // Strip nested part from owner if present
        var ownerPlusIdx = ownerBacktick.IndexOf('+');
        if (ownerPlusIdx > 0) ownerBacktick = ownerBacktick[..ownerPlusIdx];

        if (refParent == ownerBacktick)
            return refBacktick[(plusIdx + 1)..];

        return null;
    }

    private void EmitGenericObjectReferenceProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName, Dictionary<string, string> typeArgMap)
    {
        if (field.ReferenceTypeName is not null)
        {
            // Check if field type is nested inside this generic parent
            var nestedSuffix = TryGetNestedSuffix(field.ReferenceTypeName, ownerType);
            if (nestedSuffix is not null)
            {
                var sanitizedNested = SanitizeTypeName(nestedSuffix);
                if (propName == field.Name)
                    sb.AppendLine($"    public {sanitizedNested}? {propName} => Field<{sanitizedNested}>();");
                else
                    sb.AppendLine($"    public {sanitizedNested}? {propName} => Field<{sanitizedNested}>(\"{field.Name}\");");
                return;
            }

            // __Canon references that aren't nested — fall back to _.System.Object
            if (field.ReferenceTypeName.Contains("__Canon"))
            {
                if (propName == field.Name)
                    sb.AppendLine($"    public global::_.System.Object? {propName} => Field<global::_.System.Object>();");
                else
                    sb.AppendLine($"    public global::_.System.Object? {propName} => Field<global::_.System.Object>(\"{field.Name}\");");
                return;
            }
        }

        // Standard handling for non-__Canon, non-nested references
        EmitObjectReferenceProperty(sb, ownerType, field, propName);
    }

    /// <summary>
    /// Resolve an array element type name that may use backtick notation for nested types
    /// inside generics. Uses the owning type to reconstruct the concrete name.
    /// E.g., for owner "Dictionary&lt;String, Int32&gt;", element "Dictionary`2+Entry"
    /// → "Dictionary&lt;String, Int32&gt;+Entry"
    /// </summary>
    private string? ResolveArrayElementTypeName(string? elementTypeName, TypeMetadata ownerType)
    {
        if (elementTypeName is null) return null;

        // If the element type is already a known proxy, use it as-is
        if (_knownProxyTypes.Contains(elementTypeName)) return elementTypeName;

        // Check if this is a backtick-form nested type (e.g., Dictionary`2+Entry)
        if (!elementTypeName.Contains('`') || !elementTypeName.Contains('+'))
            return elementTypeName;

        // Extract the nested suffix after the last + that's after the backtick
        var plusIdx = elementTypeName.IndexOf('+');
        if (plusIdx < 0) return elementTypeName;
        var nestedSuffix = elementTypeName[(plusIdx + 1)..]; // e.g., "Entry"
        var backtickPrefix = elementTypeName[..plusIdx]; // e.g., "System.Collections.Generic.Dictionary`2"

        // Try to match against the owning type's full name
        var ownerBacktick = TypeNameHelper.ConvertToBacktickForm(ownerType.FullName);
        if (ownerBacktick == backtickPrefix)
        {
            // The nested type belongs to the owner — reconstruct with owner's concrete name
            var concreteName = ownerType.FullName + "+" + nestedSuffix;
            if (_knownProxyTypes.Contains(concreteName))
                return concreteName;
        }

        // Fallback: try the global backtick mapping
        if (_backtickToFullName.TryGetValue(elementTypeName, out var resolved))
            return resolved;

        return elementTypeName;
    }

    private void EmitArrayProperty(StringBuilder sb, TypeMetadata ownerType, FieldInfo field, string propName)
    {
        var elementKind = field.ArrayElementKind ?? FieldKind.Unknown;
        var elementTypeName = ResolveArrayElementTypeName(field.ArrayElementTypeName, ownerType);
        var fieldNameArg = propName == field.Name ? "" : $"\"{field.Name}\"";

        switch (elementKind)
        {
            case FieldKind.String:
                sb.AppendLine($"    public global::Ndump.Core.DumpArray<string?>? {propName} => ArrayField<string?>({fieldNameArg});");
                break;

            case FieldKind.Primitive:
            {
                var csType = MapClrPrimitiveTypeName(elementTypeName);
                sb.AppendLine($"    public global::Ndump.Core.DumpArray<{csType}>? {propName} => ArrayField<{csType}>({fieldNameArg});");
                break;
            }

            case FieldKind.ObjectReference:
                if (IsUsableProxyType(elementTypeName))
                {
                    var qualifiedProxy = GetFullyQualifiedProxyType(elementTypeName!);
                    sb.AppendLine($"    public global::Ndump.Core.DumpArray<{qualifiedProxy}?>? {propName} => ArrayField<{qualifiedProxy}?>({fieldNameArg});");
                }
                else
                {
                    // Unknown element type — expose raw addresses
                    sb.AppendLine($"    public global::Ndump.Core.DumpArray<ulong>? {propName} => ArrayAddresses({fieldNameArg});");
                }
                break;

            case FieldKind.ValueType:
                if (IsUsableProxyType(elementTypeName))
                {
                    var qualifiedProxy = GetFullyQualifiedProxyType(elementTypeName!);
                    sb.AppendLine($"    public global::Ndump.Core.DumpArray<{qualifiedProxy}>? {propName} => StructArrayField<{qualifiedProxy}>({fieldNameArg});");
                }
                else
                {
                    // Unknown struct element type — expose raw addresses
                    sb.AppendLine($"    public global::Ndump.Core.DumpArray<ulong>? {propName} => ArrayAddresses({fieldNameArg});");
                }
                break;

            default:
                sb.AppendLine($"    // Array field: {field.Name} ({field.TypeName}) — element type not supported");
                break;
        }
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
        // Resolve backtick-form names to their concrete angle-bracket form
        if (_backtickToFullName.TryGetValue(fullTypeName, out var concreteFullName))
            fullTypeName = concreteFullName;

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
        sb.AppendLine("    public static T Resolve<T>(ulong address, global::Ndump.Core.DumpContext ctx)");
        sb.AppendLine("    {");
        sb.AppendLine("        var typeName = ctx.GetTypeName(address);");
        sb.AppendLine("        var proxyType = typeName is not null ? ResolveProxyType(typeName) : null;");
        sb.AppendLine("        proxyType ??= typeof(T);");
        sb.AppendLine();
        sb.AppendLine("        var method = proxyType.GetMethod(\"FromAddress\", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static)");
        sb.AppendLine("            ?? throw new global::System.InvalidOperationException($\"No FromAddress factory on {proxyType}\");");
        sb.AppendLine("        return (T)method.Invoke(null, [address, ctx])!;");
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
