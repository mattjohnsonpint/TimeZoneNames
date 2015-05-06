using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SharpCompress.Reader;

namespace TimeZoneNames.DataBuilder
{
    public static class Downloader
    {
        private static readonly HttpClient HttpClientInstance = new HttpClient();

        public static async Task DownloadCldrAsync(string dir)
        {
            const string url = "http://unicode.org/Public/cldr/latest/core.zip";
            await DownloadAndExtractAsync(url, dir);
        }

        public static async Task DownloadTzdbAsync(string dir)
        {
            const string url = "http://www.iana.org/time-zones/repository/tzdata-latest.tar.gz";
            await DownloadAndExtractAsync(url, dir);
        }

        public static string GetTempDir()
        {
            return Path.GetTempPath() + Path.GetRandomFileName().Substring(0, 8);
        }

        private static async Task DownloadAndExtractAsync(string url, string dir)
        {
            using (var httpStream = await HttpClientInstance.GetStreamAsync(url))
            using (var reader = ReaderFactory.Open(httpStream))
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                
                while (reader.MoveToNextEntry())
                {
                    var entry = reader.Entry;
                    if (entry.IsDirectory)
                        continue;

                    var targetPath = Path.Combine(dir, entry.FilePath.Replace('/', '\\'));
                    var targetDir = Path.GetDirectoryName(targetPath);
                    if (targetDir == null)
                        throw new InvalidOperationException();

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    using (var stream = reader.OpenEntryStream())
                    using (var fs = File.Create(targetPath))
                    {
                        await stream.CopyToAsync(fs);
                    }
                }
            }
        }
    }
}
