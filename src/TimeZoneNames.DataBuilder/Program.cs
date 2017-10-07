using System;
using System.IO;

namespace TimeZoneNames.DataBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "data");
            var extractor = DataExtractor.Load(path, overwrite: false);
            extractor.SaveData(path);

            var filePath = Path.Combine(path, "tz.dat");
            File.Copy(filePath, @"..\TimeZoneNames\tz.dat", true);
        }
    }
}
