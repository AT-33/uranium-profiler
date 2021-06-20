using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UraniumVisualizer
{
    /// <summary>
    ///     Class to parse a profiler record file
    /// </summary>
    public class FileParser
    {
        public Dictionary<string,(int count, double maxTime)> FunctionData;
        
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
            FunctionData = new Dictionary<string, (int count, double maxTime)>();
            
            var stack = new Stack<ProfilerEvent>();
            var childrenStack = new Stack<double>();
            childrenStack.Push(0);

            var lastTimeStamp = 0;
            using (var f = new StreamReader(FileName))
            {
                while (!f.EndOfStream)
                {
                    var str = f.ReadLine() ?? "";
                    lastTimeStamp = int.Parse(Regex.Match(str, @"\d+").Value);
                    var eventType = GetEventType(str[0]);
                    var functionName = Regex.Match(str.Substring(1), @"\D+").Value;
                    var pe = new ProfilerEvent(functionName, lastTimeStamp, eventType);

                    if (pe.Type == EventType.Start)
                    {
                        stack.Push(pe);
                        childrenStack.Push(0);
                    }
                    else
                    {
                        var duration = pe.TimeStamp - stack.Peek().TimeStamp;
                        var childrenDuration = childrenStack.Pop();

                        yield return MakeFunctionRecord(functionName, stack, childrenDuration, duration);
                        childrenStack.Push(childrenStack.Pop() + duration);
                    }
                }
            }

            while (stack.Any())
            {
                var pe = stack.Pop();
                var functionName = pe.Name;
                var duration = lastTimeStamp - pe.TimeStamp;
                var childrenDuration = childrenStack.Pop();
                
                yield return MakeFunctionRecord(functionName, stack, childrenDuration, duration);
                childrenStack.Push(childrenStack.Pop() + duration);
            }
        }

        private FunctionRecord MakeFunctionRecord(string name, Stack<ProfilerEvent> stack,
            double childrenDuration, double duration)
        {
            if (FunctionData.TryGetValue(name, out var oldData))
                FunctionData[name] = (oldData.count + 1, Math.Max(duration / 1_000_000, oldData.maxTime));
            else
                FunctionData[name] = (1, duration / 1_000_000);

            return new FunctionRecord(name, stack.Count - 1,
                stack.Pop().TimeStamp, duration, duration - childrenDuration);
        }
    }
}