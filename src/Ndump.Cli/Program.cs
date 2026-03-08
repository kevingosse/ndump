using System.Diagnostics;
using Ndump.Core;
using Spectre.Console;

namespace Ndump.Cli;

internal class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
            return PrintUsage();

        try
        {
            return args[0] switch
            {
                "init" => RunInit(args[1..]),
                "build" => RunBuild(args[1..]),
                "gui" => RunGui(args[1..]),
                _ => PrintUsage()
            };
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red bold]Error:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }

    // ── init ────────────────────────────────────────────────────────────
    // Scaffold a project with proxy source files from a memory dump.
    static int RunInit(string[] args)
    {
        if (args.Length < 2)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] ndump init <dump> <output-directory>");
            return 1;
        }

        var dumpPath = args[0];
        var outputDir = args[1];

        if (!File.Exists(dumpPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Dump file not found: [yellow]{Markup.Escape(dumpPath)}[/]");
            return 1;
        }

        AnsiConsole.Write(new Rule("[blue bold]ndump init[/]").LeftJustified());
        AnsiConsole.WriteLine();

        using var context = Step("Opening dump", () => DumpContext.Open(dumpPath));

        var types = Step("Discovering types", () =>
        {
            var inspector = new TypeInspector();
            return inspector.DiscoverTypes(context);
        });
        AnsiConsole.MarkupLine($"         [dim]found[/] [bold]{types.Count}[/] [dim]type(s)[/]");

        // Clean existing generated files
        if (Directory.Exists(outputDir))
        {
            foreach (var f in Directory.GetFiles(outputDir, "*.g.cs"))
                File.Delete(f);
        }

        var generatedFiles = Step("Emitting source files", () =>
        {
            var emitter = new ProxyEmitter();
            return emitter.EmitProxies(types, outputDir);
        });
        AnsiConsole.MarkupLine($"         [dim]wrote[/] [bold]{generatedFiles.Count}[/] [dim]file(s)[/]");

        // Generate csproj if not present
        var dirName = new DirectoryInfo(outputDir).Name;
        var csprojPath = Path.Combine(outputDir, $"{dirName}.csproj");
        if (!File.Exists(csprojPath))
        {
            File.WriteAllText(csprojPath, GenerateCsproj());
            AnsiConsole.MarkupLine($"  [green]✓[/] Created [bold]{Markup.Escape(dirName)}.csproj[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [dim]–[/] [dim]{Markup.Escape(dirName)}.csproj already exists, skipped[/]");
        }

        AnsiConsole.WriteLine();
        WriteSuccess($"Project initialized at [bold]{Markup.Escape(Path.GetFullPath(outputDir))}[/]");
        return 0;
    }

    // ── build ───────────────────────────────────────────────────────────
    // Compile a proxy assembly (.dll) from a memory dump.
    static int RunBuild(string[] args)
    {
        if (args.Length < 2)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] ndump build <dump> <output-file>");
            return 1;
        }

        var dumpPath = args[0];
        var outputPath = args[1];

        if (!File.Exists(dumpPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Dump file not found: [yellow]{Markup.Escape(dumpPath)}[/]");
            return 1;
        }

        AnsiConsole.Write(new Rule("[blue bold]ndump build[/]").LeftJustified());
        AnsiConsole.WriteLine();

        using var context = Step("Opening dump", () => DumpContext.Open(dumpPath));

        var types = Step("Discovering types", () =>
        {
            var inspector = new TypeInspector();
            return inspector.DiscoverTypes(context);
        });
        AnsiConsole.MarkupLine($"         [dim]found[/] [bold]{types.Count}[/] [dim]type(s)[/]");
        AnsiConsole.WriteLine();

        WriteTypeTable(types);

        var tempDir = Path.Combine(Path.GetTempPath(), "ndump_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        try
        {
            var generatedFiles = Step("Emitting proxy sources", () =>
            {
                var emitter = new ProxyEmitter();
                return emitter.EmitProxies(types, tempDir);
            });
            AnsiConsole.MarkupLine($"         [dim]wrote[/] [bold]{generatedFiles.Count}[/] [dim]file(s)[/]");

            var result = Step("Compiling assembly", () =>
            {
                var compiler = new ProxyCompiler();
                return compiler.Compile(generatedFiles, outputPath);
            });

            if (!result.IsSuccess)
            {
                AnsiConsole.MarkupLine("  [red]✗[/] Compilation [red bold]failed[/]");
                AnsiConsole.WriteLine();

                var errorTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderStyle(Style.Parse("red"))
                    .AddColumn("[red bold]Compilation Errors[/]");

                foreach (var error in result.Errors)
                    errorTable.AddRow(Markup.Escape(error));

                AnsiConsole.Write(errorTable);
                return 1;
            }

            AnsiConsole.WriteLine();
            var fileInfo = new FileInfo(outputPath);
            WriteSuccess($"Assembly built: [bold]{Markup.Escape(Path.GetFullPath(outputPath))}[/] ({FormatSize(fileInfo.Length)})");
            return 0;
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    // ── gui ─────────────────────────────────────────────────────────────
    // Launch the graphical dump explorer.
    static int RunGui(string[] args)
    {
        if (args.Length < 1)
        {
            AnsiConsole.MarkupLine("[red]Usage:[/] ndump gui <dump>");
            return 1;
        }

        var dumpPath = Path.GetFullPath(args[0]);

        if (!File.Exists(dumpPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Dump file not found: [yellow]{Markup.Escape(dumpPath)}[/]");
            return 1;
        }

        var guiExe = FindGuiExecutable();
        if (guiExe is null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Ndump GUI executable not found.");
            AnsiConsole.MarkupLine("[dim]Ensure Ndump.Gui is built and available alongside the CLI.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Launching GUI...[/]");
        Process.Start(new ProcessStartInfo(guiExe)
        {
            ArgumentList = { dumpPath },
            UseShellExecute = false
        });
        return 0;
    }

    // ── helpers ─────────────────────────────────────────────────────────

    static T Step<T>(string description, Func<T> action)
    {
        var result = AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start(description + "...", _ => action());

        AnsiConsole.MarkupLine($"  [green]✓[/] {description}");
        return result;
    }

    static void WriteTypeTable(IReadOnlyList<TypeMetadata> types)
    {
        const int maxRows = 20;

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Type[/]")
            .AddColumn(new TableColumn("[bold]Fields[/]").Centered());

        foreach (var type in types.Take(maxRows))
            table.AddRow(Markup.Escape(type.FullName), type.Fields.Count.ToString());

        if (types.Count > maxRows)
            table.AddRow($"[dim]… and {types.Count - maxRows} more[/]", "[dim]…[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    static void WriteSuccess(string message)
    {
        AnsiConsole.Write(new Panel($"[green]{message}[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("green"))
            .Header("[green bold] ✓ Success [/]")
            .Padding(1, 0));
    }

    static string? FindGuiExecutable()
    {
        var baseDir = AppContext.BaseDirectory;
        string[] candidates =
        [
            Path.Combine(baseDir, "Ndump.Gui.exe"),
            Path.Combine(baseDir, "..", "Ndump.Gui", "Ndump.Gui.exe"),
        ];
        return candidates.FirstOrDefault(File.Exists);
    }

    static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
    };

    static string GenerateCsproj() => """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
          </PropertyGroup>

          <ItemGroup>
            <PackageReference Include="Ndump" Version="*" />
          </ItemGroup>

        </Project>
        """;

    static int PrintUsage()
    {
        AnsiConsole.Write(new FigletText("ndump").Color(Color.Blue));
        AnsiConsole.MarkupLine("[bold]Usage:[/] ndump [blue]<command>[/] [dim]<args>[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Simple)
            .HideHeaders()
            .AddColumn("Command")
            .AddColumn("Description");

        table.AddRow("[blue]init[/]  [dim]<dump> <output-dir>[/]",  "Scaffold a project with generated proxy sources");
        table.AddRow("[blue]build[/] [dim]<dump> <output-file>[/]", "Compile a proxy assembly from a memory dump");
        table.AddRow("[blue]gui[/]   [dim]<dump>[/]",               "Open the graphical dump explorer");

        AnsiConsole.Write(table);
        return 1;
    }
}
