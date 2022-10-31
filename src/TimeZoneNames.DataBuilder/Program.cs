using System.Reflection;

namespace TimeZoneNames.DataBuilder;

static class Program
{
    static void Main()
    {
        // Set the current directory so we always output data to the correct location within the solution
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var extractor = DataExtractor.Load("data", overwrite: false);

        var dataFileName = "data.json.gz";
        var outputFilePath = Path.Combine("data", dataFileName);
        extractor.SaveData(outputFilePath);

        var destPath = Path.Combine(GetSolutionDir(), "src", "TimeZoneNames", dataFileName);
        File.Copy(outputFilePath, destPath, true);
    }

    private static string GetSolutionDir()
    {
        var solutionDir = Assembly.GetExecutingAssembly().Location;
        while (!File.Exists(Path.Combine(solutionDir, "TimeZoneNames.sln")))
        {
            solutionDir = Path.GetFullPath(Path.Combine(solutionDir, ".."));
        }

        return solutionDir;
    }
}