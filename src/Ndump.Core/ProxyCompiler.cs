using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ndump.Core;

/// <summary>
/// Compiles generated proxy .cs files into an in-memory assembly using Roslyn.
/// </summary>
public sealed class ProxyCompiler
{
    /// <summary>
    /// Compile source files into an in-memory assembly.
    /// </summary>
    public CompilationResult Compile(IReadOnlyList<string> sourceFilePaths, string? outputPath = null)
    {
        var syntaxTrees = new List<SyntaxTree>();
        foreach (var path in sourceFilePaths)
        {
            var code = File.ReadAllText(path);
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, path: path));
        }

        return CompileFromTrees(syntaxTrees, outputPath);
    }

    /// <summary>
    /// Compile source code strings into an in-memory assembly.
    /// </summary>
    public CompilationResult CompileFromSource(IReadOnlyList<string> sources, string? outputPath = null)
    {
        var syntaxTrees = sources
            .Select(s => CSharpSyntaxTree.ParseText(s))
            .ToList();

        return CompileFromTrees(syntaxTrees, outputPath);
    }

    private CompilationResult CompileFromTrees(List<SyntaxTree> syntaxTrees, string? outputPath)
    {
        var assemblyName = $"Ndump.Generated_{Guid.NewGuid():N}";
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        // Use an isolated AssemblyLoadContext so multiple compilations can coexist
        var alc = new AssemblyLoadContext(assemblyName, isCollectible: true);

        if (outputPath is not null)
        {
            // Compile to disk
            var result = compilation.Emit(outputPath);
            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString())
                    .ToList();
                return CompilationResult.Failure(errors);
            }

            var assembly = alc.LoadFromAssemblyPath(Path.GetFullPath(outputPath));
            return CompilationResult.Success(assembly, outputPath);
        }
        else
        {
            // Compile to memory
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString())
                    .ToList();
                return CompilationResult.Failure(errors);
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = alc.LoadFromStream(ms);
            return CompilationResult.Success(assembly);
        }
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var refs = new List<MetadataReference>();

        // Add runtime assemblies
        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (trustedAssemblies is not null)
        {
            foreach (var path in trustedAssemblies.Split(Path.PathSeparator))
            {
                if (File.Exists(path))
                    refs.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Add Ndump.Core itself so generated code can reference DumpContext
        var coreAssembly = typeof(DumpContext).Assembly.Location;
        if (!string.IsNullOrEmpty(coreAssembly) && File.Exists(coreAssembly))
            refs.Add(MetadataReference.CreateFromFile(coreAssembly));

        return refs;
    }
}

public sealed class CompilationResult
{
    public bool IsSuccess { get; private init; }
    public Assembly? Assembly { get; private init; }
    public string? OutputPath { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static CompilationResult Success(Assembly assembly, string? outputPath = null) => new()
    {
        IsSuccess = true,
        Assembly = assembly,
        OutputPath = outputPath
    };

    public static CompilationResult Failure(IReadOnlyList<string> errors) => new()
    {
        IsSuccess = false,
        Errors = errors
    };
}
