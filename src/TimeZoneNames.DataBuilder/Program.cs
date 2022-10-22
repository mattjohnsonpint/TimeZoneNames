using System;
using System.IO;

namespace TimeZoneNames.DataBuilder;

class Program
{
    static void Main()
    {
        // Set the current directory so we always output data to the correct location within the solution
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var extractor = DataExtractor.Load("data", overwrite: false);

        var dataFileName = "data.json.gz";
        var outputFilePath = Path.Combine("data", dataFileName);
        extractor.SaveData(outputFilePath);

        var destPath = Path.Combine("..", "..", "..", "..", "TimeZoneNames", dataFileName);
        File.Copy(outputFilePath, destPath, true);
    }
}