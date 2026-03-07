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
            var proxyTypeName = MapToProxyTypeName(name);
            var proxyType = typeof(ProxyResolver).Assembly.GetType(proxyTypeName);
            if (proxyType is null) return null;

            var method = proxyType.GetMethod("FromAddress", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static);
            if (method is null) return null;

            return (global::System.Func<ulong, global::Ndump.Core.DumpContext, object>)((addr, c) => method.Invoke(null, [addr, c])!);
        });

        return factory?.Invoke(address, ctx);
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
            return "_." + ns + "." + Sanitize(name);
        }
        return "_." + Sanitize(clrTypeName);
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
