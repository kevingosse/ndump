using System.Reflection;

namespace Ndump.Core;

/// <summary>
/// Orchestrates the full dump projection pipeline:
/// load dump → inspect types → emit proxies → compile → return assembly.
/// </summary>
public sealed class DumpProjector
{
    /// <summary>
    /// Result of a dump projection.
    /// </summary>
    public sealed class ProjectionResult : IDisposable
    {
        public required DumpContext Context { get; init; }
        public required Assembly GeneratedAssembly { get; init; }
        public required IReadOnlyList<TypeMetadata> DiscoveredTypes { get; init; }
        public required IReadOnlyList<string> GeneratedFiles { get; init; }
        public required string TempDirectory { get; init; }

        public void Dispose()
        {
            Context.Dispose();
            try { if (Directory.Exists(TempDirectory)) Directory.Delete(TempDirectory, true); } catch { }
        }
    }

    /// <summary>
    /// Project a memory dump: discover types, generate proxies, compile them.
    /// </summary>
    /// <param name="dumpPath">Path to the .dmp file</param>
    /// <param name="targetPath">Target path (used for output context, not for temp files)</param>
    public ProjectionResult Project(string dumpPath, string? targetPath = null)
    {
        // 1. Create temp folder
        var tempDir = Path.Combine(Path.GetTempPath(), "ndump_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        // 2. Open dump
        var context = DumpContext.Open(dumpPath);

        try
        {
            // 3. Inspect types
            var inspector = new TypeInspector();
            var types = inspector.DiscoverTypes(context);

            // 4. Emit proxies
            var emitter = new ProxyEmitter();
            var generatedFiles = emitter.EmitProxies(types, tempDir);

            // 5. Compile
            var compiler = new ProxyCompiler();
            var outputDll = Path.Combine(tempDir, "Ndump.Generated.dll");
            var result = compiler.Compile(generatedFiles, outputDll);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Failed to compile generated proxies:\n" + string.Join("\n", result.Errors));
            }

            return new ProjectionResult
            {
                Context = context,
                GeneratedAssembly = result.Assembly!,
                DiscoveredTypes = types,
                GeneratedFiles = generatedFiles,
                TempDirectory = tempDir
            };
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }
}
