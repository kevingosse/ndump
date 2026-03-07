using Ndump.Core;

namespace Ndump.Cli;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            PrintUsage();
            return 1;
        }

        return args[0] switch
        {
            "generate" => RunGenerate(args[1..]),
            _ => PrintUsage()
        };
    }

    static int RunGenerate(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: ndump generate <dump-path> [-o <output-path>]");
            return 1;
        }

        var dumpPath = args[0];
        string? outputPath = null;

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] is "-o" or "--output" && i + 1 < args.Length)
            {
                outputPath = args[++i];
            }
        }

        outputPath ??= Path.Combine(Directory.GetCurrentDirectory(), "Ndump.Generated.dll");

        Console.WriteLine($"Opening dump: {dumpPath}");
        using var context = DumpContext.Open(dumpPath);

        var inspector = new TypeInspector();
        var types = inspector.DiscoverTypes(context);
        Console.WriteLine($"Discovered {types.Count} type(s)");

        foreach (var type in types)
            Console.WriteLine($"  {type.FullName} ({type.Fields.Count} fields)");

        var tempDir = Path.Combine(Path.GetTempPath(), "ndump_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var emitter = new ProxyEmitter();
        var generatedFiles = emitter.EmitProxies(types, tempDir);

        var compiler = new ProxyCompiler();
        var result = compiler.Compile(generatedFiles, outputPath);

        if (!result.IsSuccess)
        {
            Console.Error.WriteLine("Compilation failed:");
            foreach (var error in result.Errors)
                Console.Error.WriteLine($"  {error}");
            return 1;
        }

        Console.WriteLine($"Generated: {outputPath}");
        return 0;
    }

    static int PrintUsage()
    {
        Console.WriteLine("Usage: ndump <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  generate <dump-path> [-o <output-path>]  Generate proxy DLL from a memory dump");
        return 1;
    }
}
