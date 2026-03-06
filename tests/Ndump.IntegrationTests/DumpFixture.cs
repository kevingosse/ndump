using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ndump.IntegrationTests;

/// <summary>
/// Helper to build TestApp, start it, capture a memory dump, and clean up.
/// </summary>
public sealed class DumpFixture : IAsyncLifetime
{
    public string DumpPath { get; private set; } = "";

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

        // Start TestApp
        var testAppExe = Path.Combine(_publishDir, "Ndump.TestApp.exe");
        if (!File.Exists(testAppExe))
            testAppExe = Path.Combine(_publishDir, "Ndump.TestApp.dll");

        ProcessStartInfo startInfo;
        if (testAppExe.EndsWith(".exe"))
        {
            startInfo = new ProcessStartInfo
            {
                FileName = testAppExe,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
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
                Arguments = testAppExe,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        var testApp = Process.Start(startInfo)!;

        try
        {
            // Wait for READY signal
            var readyLine = await testApp.StandardOutput.ReadLineAsync();
            if (readyLine is null || !readyLine.StartsWith("READY:"))
                throw new InvalidOperationException($"TestApp did not signal ready. Got: {readyLine}");

            var pid = testApp.Id;

            // Capture dump using MiniDumpWriteDump Win32 API
            DumpPath = Path.Combine(Path.GetTempPath(), $"ndump_test_{Guid.NewGuid():N}.dmp");
            WriteMiniDump(pid, DumpPath);

            if (!File.Exists(DumpPath))
                throw new FileNotFoundException("Dump file was not created", DumpPath);
        }
        finally
        {
            // Terminate TestApp
            try
            {
                testApp.StandardInput.WriteLine("quit");
                if (!testApp.WaitForExit(3000))
                    testApp.Kill();
            }
            catch
            {
                try { testApp.Kill(); } catch { }
            }
        }
    }

    public Task DisposeAsync()
    {
        try { if (File.Exists(DumpPath)) File.Delete(DumpPath); } catch { }
        try { if (Directory.Exists(_publishDir)) Directory.Delete(_publishDir, true); } catch { }
        return Task.CompletedTask;
    }

    private static void WriteMiniDump(int pid, string dumpPath)
    {
        using var process = Process.GetProcessById(pid);
        using var fs = new FileStream(dumpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        // MiniDumpWithFullMemory = 0x00000002
        const int MiniDumpWithFullMemory = 0x00000002;

        bool success = MiniDumpWriteDump(
            process.Handle,
            (uint)pid,
            fs.SafeFileHandle.DangerousGetHandle(),
            MiniDumpWithFullMemory,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (!success)
        {
            var hr = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"MiniDumpWriteDump failed with error code 0x{hr:X8}");
        }
    }

    [DllImport("dbghelp.dll", SetLastError = true)]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        IntPtr hFile,
        int dumpType,
        IntPtr exceptionParam,
        IntPtr userStreamParam,
        IntPtr callbackParam);

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
