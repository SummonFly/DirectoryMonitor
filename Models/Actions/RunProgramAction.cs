using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMonitor.Models.Actions
{
    public class RunProgramAction : ActionBase
    {
        public string ProgramPath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;

        public override async Task ExecuteAsync(FileSystemEventArgs args, CancellationToken cancellationToken = default)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ProgramPath,
                    Arguments = Arguments,
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                await Task.Run(() => Process.Start(startInfo), cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to run program: {ex.Message}");
                throw;
            }
        }
    }
}
