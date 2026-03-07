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
        var assemblyName = "Ndump.Generated";
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

        // Use reference assemblies from the targeting pack instead of runtime assemblies.
        // Runtime assemblies (TRUSTED_PLATFORM_ASSEMBLIES) expose System.Private.CoreLib
        // directly, which causes consumers of the generated DLL to need an explicit
        // reference to System.Private.CoreLib. Reference assemblies use proper type
        // forwarding through System.Runtime.
        var refAssemblyDir = GetReferenceAssemblyDirectory();
        if (refAssemblyDir is not null)
        {
            foreach (var dll in Directory.GetFiles(refAssemblyDir, "*.dll"))
            {
                refs.Add(MetadataReference.CreateFromFile(dll));
            }
        }
        else
        {
            // Fallback: use runtime assemblies but exclude System.Private.CoreLib
            var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (trustedAssemblies is not null)
            {
                foreach (var path in trustedAssemblies.Split(Path.PathSeparator))
                {
                    if (File.Exists(path))
                        refs.Add(MetadataReference.CreateFromFile(path));
                }
            }
        }

        // Add Ndump.Core itself so generated code can reference DumpContext
        var coreAssembly = typeof(DumpContext).Assembly.Location;
        if (!string.IsNullOrEmpty(coreAssembly) && File.Exists(coreAssembly))
            refs.Add(MetadataReference.CreateFromFile(coreAssembly));

        return refs;
    }

    private static string? GetReferenceAssemblyDirectory()
    {
        // Locate the .NET reference assemblies from the targeting pack.
        // Path: {dotnet_root}/packs/Microsoft.NETCore.App.Ref/{version}/ref/net{major}.{minor}/
        var dotnetRoot = Path.GetDirectoryName(Path.GetDirectoryName(typeof(object).Assembly.Location));
        if (dotnetRoot is null)
            return null;

        // Walk up from the runtime directory to the dotnet root
        // Runtime location is typically: {dotnet_root}/shared/Microsoft.NETCore.App/{version}/
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimeDir is null)
            return null;

        var runtimeVersion = Path.GetFileName(runtimeDir); // e.g. "10.0.0"
        var dotnetRootDir = Path.GetFullPath(Path.Combine(runtimeDir, "..", "..", ".."));
        var packsDir = Path.Combine(dotnetRootDir, "packs", "Microsoft.NETCore.App.Ref");

        if (!Directory.Exists(packsDir))
            return null;

        // Try to find a matching major.minor version
        var version = Version.Parse(runtimeVersion);
        var tfm = $"net{version.Major}.{version.Minor}";

        // Look for the best matching pack version
        var packVersionDirs = Directory.GetDirectories(packsDir)
            .Select(d => (Path: d, Name: Path.GetFileName(d)))
            .Where(d => Version.TryParse(d.Name, out var v) && v.Major == version.Major && v.Minor == version.Minor)
            .OrderByDescending(d => d.Name)
            .ToList();

        foreach (var packDir in packVersionDirs)
        {
            var refDir = Path.Combine(packDir.Path, "ref", tfm);
            if (Directory.Exists(refDir))
                return refDir;
        }

        return null;
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
