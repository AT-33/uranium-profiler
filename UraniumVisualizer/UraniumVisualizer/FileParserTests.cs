using System.IO;
using System.Text;
using NUnit.Framework;

namespace UraniumVisualizer
{
    [TestFixture]
    class FileParserTests
    {
        [TestCase(@"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test1.ups",
                  @"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test1Solve.txt")]
        [TestCase(@"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test2.ups",
                  @"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test2Solve.txt")]

        public void TestCases(string testPath, string testSolvePath)
        {
            var p = new FileParser(testPath);
            var testReturn = new StringBuilder();
            foreach (var e in p.Parse())
            {
                var t = new string('\t', e.PositionY);
                testReturn.Append(t + $"Name: {e.Name}, PositionX: {e.PositionX}, " +
                          $"PositionY: {e.PositionY}, Duration: {e.Duration}\r\n");
            }
            Assert.AreEqual(testReturn.ToString(), File.ReadAllText(testSolvePath));
        }
    }
}
