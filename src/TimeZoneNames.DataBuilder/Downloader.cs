using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SharpCompress.Readers;

namespace TimeZoneNames.DataBuilder
{
    public static class Downloader
    {
        private static readonly HttpClient HttpClientInstance = new HttpClient();

        public static async Task DownloadCldrAsync(string dir)
        {
            const string url = "https://unicode.org/Public/cldr/latest/core.zip";
            await DownloadAndExtractAsync(url, dir);

            // use the trunk metazones, as they tend to be more frequently updated
            const string url2 = "https://unicode.org/repos/cldr/trunk/common/supplemental/metaZones.xml";
            await DownloadAsync(url2, Path.Combine(dir, @"common\supplemental"));
        }

        public static async Task DownloadNzdAsync(string dir)
        {
            const string url = "https://nodatime.org/tzdb/latest.txt";
            using (var result = await HttpClientInstance.GetAsync(url))
            {
                var dataUrl = (await result.Content.ReadAsStringAsync()).TrimEnd();
                await DownloadAsync(dataUrl, dir);
            }
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
