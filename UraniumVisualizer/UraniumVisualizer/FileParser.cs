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
            switch (c)
            {
                case 'B':
                    return EventType.Start;
                case 'E':
                    return EventType.End;
                default:
                    throw new SystemException("Could not determine the type of event");
            }
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
                while (!f.EndOfStream)
                {
                    var str = f.ReadLine();
                    //PE, pe - ProfilerEvent
                    var timeStampPE = int.Parse(Regex.Match(str, @"\d+").Value);
                    var typePE = GetEventType(str[0]);
                    var pe = new ProfilerEvent(timeStampPE, typePE);
                    var functionName = Regex.Match(str.Substring(1), @"\D+").Value;

                    if (stack.Count == 0 || pe.Type == EventType.Start)
                    {
                        yield return new FunctionRecord(functionName, stack.Count,
                                                        pe.TimeStamp, double.PositiveInfinity);
                        stack.Push(pe);
                    }
                    else
                    {
                        var duration = pe.TimeStamp - stack.Peek().TimeStamp;
                        yield return new FunctionRecord(functionName, stack.Count,
                                                        stack.Pop().TimeStamp, duration);
                    }
                }
            }
        }
    }
}