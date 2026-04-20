using System.IO;

namespace DirectoryMonitor.Models.Actions
{
    public class CopyFileAction : ActionBase
    {
        public string DestinationFolder { get; set; } = string.Empty;
        public bool Overwrite { get; set; } = false;

        public override async Task ExecuteAsync(FileSystemEventArgs args, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Directory.Exists(DestinationFolder))
                    Directory.CreateDirectory(DestinationFolder);

                var fileName = Path.GetFileName(args.FullPath);
                var destPath = Path.Combine(DestinationFolder, fileName);

                await Task.Run(() => File.Copy(args.FullPath, destPath, Overwrite), cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to copy file: {ex.Message}");
                throw;
            }
        }
    }
}
