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

            var dataFileName = "data.json.gz";
            var outputFilePath = Path.Combine(path, dataFileName);
            extractor.SaveData(outputFilePath);

            File.Copy(outputFilePath, $@"..\TimeZoneNames\{dataFileName}", true);
        }
    }
}
