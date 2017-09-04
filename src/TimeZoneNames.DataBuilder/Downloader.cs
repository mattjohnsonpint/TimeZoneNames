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

            // use the trunk windows mappings and metazones, as they tend to be more accurate and frequently updated
            const string url2 = "http://unicode.org/repos/cldr/trunk/common/supplemental/windowsZones.xml";
            await DownloadAsync(url2, Path.Combine(dir, @"common\supplemental"));

            const string url3 = "http://unicode.org/repos/cldr/trunk/common/supplemental/metaZones.xml";
            await DownloadAsync(url3, Path.Combine(dir, @"common\supplemental"));
        }

        public static async Task DownloadTzdbAsync(string dir)
        {
            const string url = "http://www.iana.org/time-zones/repository/tzdata-latest.tar.gz";
            await DownloadAndExtractAsync(url, dir);
        }

        public static async Task DownloadNzdAsync(string dir)
        {
            const string url = "http://nodatime.org/tzdb/latest.txt";
            using (var result = await HttpClientInstance.GetAsync(url))
            {
                var dataUrl = (await result.Content.ReadAsStringAsync()).TrimEnd();
                await DownloadAsync(dataUrl, dir);
            }
        }

        public static string GetTempDir()
        {
            return Path.GetTempPath() + Path.GetRandomFileName().Substring(0, 8);
        }

        private static async Task DownloadAsync(string url, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filename = url.Substring(url.LastIndexOf('/') + 1);
            using (var result = await HttpClientInstance.GetAsync(url))
            using (var fs = File.Create(Path.Combine(dir, filename)))
            {
                await result.Content.CopyToAsync(fs);
            }
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

                    var targetPath = Path.Combine(dir, entry.Key.Replace('/', '\\'));
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
