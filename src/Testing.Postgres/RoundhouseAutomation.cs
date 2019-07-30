using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rocket.Surgery.LocalDevelopment
{
    public class RoundhouseAutomation
    {
        private readonly List<string> _logs = new List<string>();

        public static RoundhouseAutomation ForLocalDevelopment(string directory = null)
        {
            return new RoundhouseAutomation(directory);
        }

        public static RoundhouseAutomation ForUnitTesting(Type type, string directory = null)
        {
            return ForUnitTesting(type.Assembly, directory);
        }

        public static RoundhouseAutomation ForUnitTesting(Assembly type, string directory = null)
        {
            return new RoundhouseAutomation(directory);
        }

        private readonly string _directory;

        private RoundhouseAutomation(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Directory.GetCurrentDirectory();
                while (!Directory.Exists(Path.Combine(directory, ".git")))
                {
                    directory = Path.GetDirectoryName(directory);
                }
            }

            _directory = directory;
        }

        public IEnumerable<string> Logs => _logs;

        public async Task Start(string connectionString)
        {
            _logs.Add("Bringing database up to date");
            await Task.Yield();
            _logs.Add(Roundhouse(connectionString));
        }

        private string Roundhouse(string connectionString)
        {
            var roundhouse = Directory.EnumerateFiles(
                Path.Combine(_directory, "tools"),
                "rh.exe",
                SearchOption.AllDirectories
            ).FirstOrDefault();

            var items = new Dictionary<string, string>()
            {
                { "-c", connectionString },
                { "-f", Path.Combine(_directory, "database") },
                { "-dt", "postgres" },
                // { "--version", new GitVersionAutomation().LegacySemVerPadded },
            };
            var arguments = string.Join(" ", items.Select(x => $"{x.Key} \"{x.Value}\""));
            arguments += " --silent";

            var process = Process.Start(new ProcessStartInfo(roundhouse)
            {
                Arguments = arguments,
                WorkingDirectory = _directory,
                RedirectStandardOutput = true,
            });

            var content = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return content;
        }
    }
}
