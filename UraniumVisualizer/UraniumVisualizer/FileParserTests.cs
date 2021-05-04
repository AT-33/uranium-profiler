using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UraniumVisualizer
{
    [TestFixture]
    class FileParserTests
    {
        const string Folder = @"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\";

        [TestCase(@"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test1.ups",
                  @"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test1Solve.txt")]
        [TestCase(@"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test2.ups",
                  @"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\test2Solve.txt")]

        public void TestCases(string testPath, string testSolvePath)
        {
            var p = new FileParser(testPath);
            var testName = Regex.Match(testPath, @"\\test\d+").Value;
            var testReturnPath = $@"{Folder}{testName}Return.txt";
            CreatTestRerurnInFolder(p, testReturnPath);
            Assert.AreEqual(ReadFile(testReturnPath), ReadFile(testSolvePath));
        }

        private static void CreatTestRerurnInFolder(FileParser p, string testReturnPath)
        {
            using (var fstream = new FileStream(testReturnPath, FileMode.OpenOrCreate))
            {
                foreach (var e in p.Parse())
                {
                    var t = new string('\t', e.PositionY);
                    var arr = Encoding.Default
                        .GetBytes(t + $"Name: {e.Name}, PositionX: {e.PositionX}, " +
                                  $"PositionY: {e.PositionY}, Duration: {e.Duration}\r\n");
                    fstream.Write(arr, 0, arr.Length);
                }
            }
        }

        private static string ReadFile(string path)
        {
            using (FileStream fstream = File.OpenRead(path))
            {
                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                return Encoding.Default.GetString(array);
            }
        }
    }
}
