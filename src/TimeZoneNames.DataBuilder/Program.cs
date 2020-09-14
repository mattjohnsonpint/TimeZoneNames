using System.IO;

namespace TimeZoneNames.DataBuilder
{
    class Program
    {
        static void Main()
        {
            string path = Path.Combine(".", "data");
            var extractor = DataExtractor.Load("data", overwrite: false);

            var dataFileName = "data.json.gz";
            string outputFilePath = Path.Combine(path, dataFileName);
            extractor.SaveData(outputFilePath);

            string destPath = Path.Combine("..", "..", "..", "..", "TimeZoneNames", dataFileName);
            File.Copy(outputFilePath, destPath, true);
        }
    }
}
