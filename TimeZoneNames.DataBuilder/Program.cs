using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Threading;

namespace TimeZoneNames.DataBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncPump.Run(DownloadAndBuildAsync);
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
