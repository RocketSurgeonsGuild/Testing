using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Rocket.Surgery.Extensions.Testing.AzureStorageEmulator
{
    public class AzureStorageEmulatorAutomation : IDisposable
    {
        public bool StartedByAutomation { get; private set; }

        public void Dispose()
        {
            if (StartedByAutomation)
            {
                Stop();
            }
        }

        public void Start()
        {
            if (!IsEmulatorRunning())
            {
                RunWithParameter("start");
                StartedByAutomation = true;
            }
        }

        public void Stop()
        {
            RunWithParameter("stop");
        }

        public void ClearAll()
        {
            RunWithParameter("clear all");
        }

        public void ClearBlobs()
        {
            RunWithParameter("clear blob");
        }

        public void ClearTables()
        {
            RunWithParameter("clear table");
        }

        public void ClearQueues()
        {
            RunWithParameter("clear queue");
        }

        public static bool IsEmulatorRunning()
        {
            var path = GetPathToStorageEmulatorExecutable();

            var output = ProcessHelper.RunAndGetStandardOutputAsString(path, "status");

            if (output.Contains("IsRunning: True"))
            {
                return true;
            }
            else if (output.Contains("IsRunning: False"))
            {
                return false;
            }

            throw new ApplicationException("Unable to determine if Azure Storage Emulator is running.");
        }

        private static void RunWithParameter(string parameter)
        {
            var path = GetPathToStorageEmulatorExecutable();

            ProcessHelper.RunAndGetStandardOutputAsString(path, parameter);
        }

        private static string AzureSdkDirectory
        {
            get
            {
                var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var path = Path.Combine(programFilesX86, @"Microsoft SDKs\Azure");

                return path;
            }
        }

        private static string GetPathToStorageEmulatorExecutable()
        {
            var paths = new[]
            {
                Path.Combine(AzureSdkDirectory, @"Storage Emulator\AzureStorageEmulator.exe"),
                Path.Combine(AzureSdkDirectory, @"Storage Emulator\WAStorageEmulator.exe")
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException(
                "Unable to locate Azure storage emulator at any of the expected paths.",
                string.Join(", ", paths));
        }

        class ProcessHelper
        {
            public static string RunAndGetStandardOutputAsString(string path, string parameter)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(path, parameter)
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();

                var sr = process.StandardOutput;
                var output = sr.ReadToEnd();

                process.WaitForExit();

                return output;
            }
        }
    }
}
