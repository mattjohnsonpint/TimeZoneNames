//using System;
//using System.IO;
//using System.Threading.Tasks;
//using TimeZoneNames.DataBuilder;
//using Xunit;

//namespace TimeZoneNames.Tests
//{
//    public class DataBuilderTests
//    {

//        [Fact]
//        public async Task Can_Build_Data()
//        {
//            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
//            var extractor = await DataExtractor.LoadAsync(path);
//            extractor.SaveData(path);

//            Assert.True(File.Exists(Path.Combine(path, @"cldr\unicode-license.txt")));
//            Assert.True(File.Exists(Path.Combine(path, @"tzdb\zone.tab")));
//            Assert.True(File.Exists(Path.Combine(path, @"tz.dat")));

//            var info = new FileInfo(Path.Combine(path, "tz.dat"));
//            Assert.True(info.Exists);
//            Assert.True(info.Length > 0);
//        }
//    }
//}
