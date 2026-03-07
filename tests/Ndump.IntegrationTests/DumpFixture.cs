using System.Diagnostics;
using System.Reflection;
using Ndump.Core;

namespace Ndump.IntegrationTests;

/// <summary>
/// Helper to build TestApp, start it (which self-dumps via createdump), and clean up.
/// Also projects the dump once so all tests share a single projection.
/// </summary>
public sealed class DumpFixture : IAsyncLifetime
{
    public string DumpPath { get; private set; } = "";

    /// <summary>
    /// Shared projection result. Tests should use this instead of calling Project() themselves.
    /// </summary>
    public DumpProjector.ProjectionResult Projection { get; private set; } = null!;

    private readonly string _testAppDir;
    private readonly string _publishDir;

    public DumpFixture()
    {
        var solutionRoot = FindSolutionRoot();
        _testAppDir = Path.Combine(solutionRoot, "src", "Ndump.TestApp");
        _publishDir = Path.Combine(Path.GetTempPath(), $"ndump_testapp_{Guid.NewGuid():N}");
    }

    public async Task InitializeAsync()
    {
        // Publish TestApp
        Directory.CreateDirectory(_publishDir);
        var publish = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish \"{_testAppDir}\" -o \"{_publishDir}\" -c Release --no-self-contained",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;

        await publish.WaitForExitAsync();
        if (publish.ExitCode != 0)
        {
            var stderr = await publish.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to publish TestApp: {stderr}");
        }

        // Determine dump path
        DumpPath = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}.dmp");

        // Start TestApp — it will dump itself and exit
        var testAppExe = Path.Combine(_publishDir, "Ndump.TestApp.exe");
        if (!File.Exists(testAppExe))
            testAppExe = Path.Combine(_publishDir, "Ndump.TestApp.dll");

        ProcessStartInfo startInfo;
        if (testAppExe.EndsWith(".exe"))
        {
            startInfo = new ProcessStartInfo
            {
                FileName = testAppExe,
                Arguments = $"\"{DumpPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else
        {
            startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{testAppExe}\" \"{DumpPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        var testApp = Process.Start(startInfo)!;
        await testApp.WaitForExitAsync();

        if (testApp.ExitCode != 0)
        {
            var stderr = await testApp.StandardError.ReadToEndAsync();
            var stdout = await testApp.StandardOutput.ReadToEndAsync();
            throw new InvalidOperationException(
                $"TestApp failed (exit {testApp.ExitCode}). stderr: {stderr} stdout: {stdout}");
        }

        if (!File.Exists(DumpPath))
            throw new FileNotFoundException("Dump file was not created", DumpPath);

        // Project once for all tests
        var projector = new DumpProjector();
        Projection = projector.Project(DumpPath);
    }

    public Task DisposeAsync()
    {
        try { Projection?.Dispose(); } catch { }
        try { if (File.Exists(DumpPath)) File.Delete(DumpPath); } catch { }
        try { if (Directory.Exists(_publishDir)) Directory.Delete(_publishDir, true); } catch { }
        return Task.CompletedTask;
    }

    private static string FindSolutionRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "ndump.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException("Could not find solution root (ndump.slnx)");
    }
}
