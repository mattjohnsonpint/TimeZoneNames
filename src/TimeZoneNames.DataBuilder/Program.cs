using System.IO;

namespace TimeZoneNames.DataBuilder;

class Program
{
    static void Main()
    {
        var path = Path.Combine(".", "data");
        var extractor = DataExtractor.Load("data", overwrite: false);

        var dataFileName = "data.json.gz";
        var outputFilePath = Path.Combine(path, dataFileName);
        extractor.SaveData(outputFilePath);

        var destPath = Path.Combine("..", "..", "..", "..", "TimeZoneNames", dataFileName);
        File.Copy(outputFilePath, destPath, true);
    }
}