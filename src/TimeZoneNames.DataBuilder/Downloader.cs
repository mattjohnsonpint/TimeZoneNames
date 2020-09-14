using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SharpCompress.Common;
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

            // use the latest dev version of the metazones, as they tend to be more frequently updated
            const string url2 = "https://raw.githubusercontent.com/unicode-org/cldr/master/common/supplemental/metaZones.xml";
            await DownloadAsync(url2, Path.Combine(dir, @"common\supplemental"));
        }

        public static async Task DownloadNzdAsync(string dir)
        {
            const string url = "https://nodatime.org/tzdb/latest.txt";
            using HttpResponseMessage result = await HttpClientInstance.GetAsync(url);
            string dataUrl = (await result.Content.ReadAsStringAsync()).TrimEnd();
            await DownloadAsync(dataUrl, dir);
        }

        public static async Task DownloadTZResAsync(string dir)
        {
            const string url = "https://raw.githubusercontent.com/tomkludy/TimeZoneWindowsResourceExtractor/master/TZResScraper/tzinfo.json";
            await DownloadAsync(url, dir);
        }

        private static async Task DownloadAsync(string url, string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string filename = url.Substring(url.LastIndexOf('/') + 1);
            using HttpResponseMessage result = await HttpClientInstance.GetAsync(url);
            await using FileStream fs = File.Create(Path.Combine(dir, filename));
            await result.Content.CopyToAsync(fs);
        }

        private static async Task DownloadAndExtractAsync(string url, string dir)
        {
            await using Stream httpStream = await HttpClientInstance.GetStreamAsync(url);
            using IReader reader = ReaderFactory.Open(httpStream);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            while (reader.MoveToNextEntry())
            {
                IEntry entry = reader.Entry;
                if (entry.IsDirectory)
                    continue;

                string targetPath = Path.Combine(dir, entry.Key.Replace('/', '\\'));
                string targetDir = Path.GetDirectoryName(targetPath);
                if (targetDir == null)
                    throw new InvalidOperationException();

                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                await using EntryStream stream = reader.OpenEntryStream();
                await using FileStream fs = File.Create(targetPath);
                await stream.CopyToAsync(fs);
            }
        }
    }
}
