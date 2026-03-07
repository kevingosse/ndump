#nullable enable
namespace _;

internal static class ProxyResolver
{
    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<string, global::System.Func<ulong, global::Ndump.Core.DumpContext, object>?> _cache = new();

    public static object? Resolve(ulong address, global::Ndump.Core.DumpContext ctx)
    {
        var typeName = ctx.GetTypeName(address);
        if (typeName is null) return null;

        var factory = _cache.GetOrAdd(typeName, static name =>
        {
            var proxyType = ResolveProxyType(name);
            if (proxyType is null) return null;

            var method = proxyType.GetMethod("FromAddress", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
            if (method is null) return null;

            return (global::System.Func<ulong, global::Ndump.Core.DumpContext, object>)((addr, c) => method.Invoke(null, [addr, c])!);
        });

        return factory?.Invoke(address, ctx);
    }

    private static global::System.Type? ResolveProxyType(string clrTypeName)
    {
        var asm = typeof(ProxyResolver).Assembly;

        // Try non-generic lookup first
        var proxyTypeName = MapToProxyTypeName(clrTypeName);
        var proxyType = asm.GetType(proxyTypeName);
        if (proxyType is not null) return proxyType;

        // Try generic lookup: parse type args and construct closed generic
        var firstAngle = clrTypeName.IndexOf('<');
        if (firstAngle < 0) return null;

        var typeArgs = ParseTypeArgs(clrTypeName, firstAngle);
        var defName = clrTypeName[..firstAngle];

        // Find the generic definition using backtick notation
        var lastDot = defName.LastIndexOf('.');
        var ns = lastDot > 0 ? defName[..lastDot] : "";
        var shortName = lastDot > 0 ? defName[(lastDot + 1)..] : defName;
        var proxyNs = ns.Length > 0 ? "_." + ns : "_";
        var genDefTypeName = proxyNs + "." + shortName + "`" + typeArgs.Count;

        var genDefType = asm.GetType(genDefTypeName);
        if (genDefType is null) return null;

        // Map each CLR type arg to its proxy type
        var proxyArgTypes = new global::System.Type[typeArgs.Count];
        for (int i = 0; i < typeArgs.Count; i++)
        {
            proxyArgTypes[i] = MapClrTypeArgToProxyType(typeArgs[i]);
        }

        try { return genDefType.MakeGenericType(proxyArgTypes); }
        catch { return null; }
    }

    private static global::System.Type MapClrTypeArgToProxyType(string clrTypeName)
    {
        // Primitives and well-known types
        var t = clrTypeName switch
        {
            "System.String" => typeof(string),
            "System.Boolean" => typeof(bool),
            "System.Byte" => typeof(byte),
            "System.SByte" => typeof(sbyte),
            "System.Int16" => typeof(short),
            "System.UInt16" => typeof(ushort),
            "System.Int32" => typeof(int),
            "System.UInt32" => typeof(uint),
            "System.Int64" => typeof(long),
            "System.UInt64" => typeof(ulong),
            "System.Single" => typeof(float),
            "System.Double" => typeof(double),
            "System.Char" => typeof(char),
            "System.IntPtr" => typeof(nint),
            "System.UIntPtr" => typeof(nuint),
            _ => null
        };
        if (t is not null) return t;

        // Try resolving as a proxy type (recursive for nested generics)
        var proxyType = ResolveProxyType(clrTypeName);
        if (proxyType is not null) return proxyType;

        // Fallback: try the real CLR type
        return global::System.Type.GetType(clrTypeName) ?? typeof(object);
    }

    private static global::System.Collections.Generic.List<string> ParseTypeArgs(string name, int firstAngle)
    {
        var args = new global::System.Collections.Generic.List<string>();
        int depth = 0, start = firstAngle + 1;
        for (int i = firstAngle; i < name.Length; i++)
        {
            if (name[i] == '<') depth++;
            else if (name[i] == '>')
            {
                depth--;
                if (depth == 0) { args.Add(name[start..i].Trim()); break; }
            }
            else if (name[i] == ',' && depth == 1)
            {
                args.Add(name[start..i].Trim());
                start = i + 1;
            }
        }
        return args;
    }

    private static string MapToProxyTypeName(string clrTypeName)
    {
        var genericStart = clrTypeName.Length;
        for (int i = 0; i < clrTypeName.Length; i++)
        {
            if (clrTypeName[i] is '<' or '`' or '[')
            {
                genericStart = i;
                break;
            }
        }

        var lastDot = clrTypeName.LastIndexOf('.', genericStart - 1);
        if (lastDot > 0)
        {
            var ns = clrTypeName[..lastDot];
            var name = clrTypeName[(lastDot + 1)..];
            return "_." + ns + "." + SanitizeNested(name);
        }
        return "_." + SanitizeNested(clrTypeName);
    }

    private static string SanitizeNested(string name)
    {
        // Split on + outside <>, sanitize each part, rejoin with + for CLR nested type lookup
        var parts = new global::System.Collections.Generic.List<string>();
        int depth = 0, start = 0;
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '<') depth++;
            else if (name[i] == '>') depth--;
            else if (name[i] == '+' && depth == 0)
            {
                parts.Add(Sanitize(name[start..i]));
                start = i + 1;
            }
        }
        parts.Add(Sanitize(name[start..]));
        return string.Join("+", parts);
    }

    private static string Sanitize(string name)
    {
        return name
            .Replace('<', '_').Replace('>', '_')
            .Replace(',', '_').Replace(' ', '_')
            .Replace('+', '_').Replace('.', '_')
            .Replace('`', '_').Replace('[', '_')
            .Replace(']', '_');
    }
}
