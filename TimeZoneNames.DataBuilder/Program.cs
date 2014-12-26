using System;
using System.IO;
using System.Threading.Tasks;

namespace TimeZoneNames.DataBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            DownloadAndBuildAsync().Wait();
        }

        private static async Task DownloadAndBuildAsync()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var extractor = await DataExtractor.LoadAsync(path);
            extractor.SaveData(path);
            
            // Copy to PCL project for embedding
            var filePath = Path.Combine(path, "tz.dat");
            File.Copy(filePath, @"..\..\..\TimeZoneNames\tz.dat", true);
        }
    }
}
