using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UraniumVisualizer
{
    /// <summary>
    ///     Class to parse a profiler record file
    /// </summary>
    public class FileParser
    {
        private string FileName { get; }

        public FileParser(string fileName)
        {
            FileName = fileName;
        }

        private static EventType GetEventType(char c)
        {
            if (c == 'B')
                return EventType.Start;
            if (c == 'E')
                return EventType.End;
            throw new SystemException("Could not determine the type of event");
        }

        /// <summary>
        ///     Lazily enumerate all profiler events from the file
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FunctionRecord> Parse()
        {
            var stack = new Stack<ProfilerEvent>();

            using (var f = new StreamReader(FileName))
            {
                var functionPosition = 0;

                while (!f.EndOfStream)
                {
                    var str = f.ReadLine();
                    //PE, pe - ProfilerEvent
                    var timeStampPE = int.Parse(Regex.Match(str, @"\d+").Value);
                    var typePE = GetEventType(str[0]);
                    var pe = new ProfilerEvent(timeStampPE, typePE);

                    if (stack.Count == 0 || pe.Type == EventType.Start)
                    {
                        stack.Push(pe);
                        var functionName = Regex.Match(str.Substring(1), @"\D+").Value;
                        yield return new FunctionRecord(functionName, functionPosition, pe.TimeStamp);
                        functionPosition++;
                    }
                    else
                    {
                        stack.Pop();
                        functionPosition--;
                    }
                }
            }
        }

        public void Test()
        {
            using (var fstream = new FileStream(@"H:\GitH\uranium-profiler\UraniumVisualizer\UraniumVisualizer\ParserTests\testReturn.txt",
                                                FileMode.OpenOrCreate))
            {
                foreach (var e in Parse())
                {
                    var arr = System.Text
                        .Encoding.Default
                        .GetBytes($"Name: {e.Name}\n    PositionX: {e.PositionX}, PositionY: {e.PositionY}\n\n");
                    fstream.Write(arr, 0, arr.Length);
                }
            }
        }
    }
}