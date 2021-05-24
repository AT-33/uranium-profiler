using System;

namespace UraniumVisualizer
{
    public readonly struct ProfilerEvent
    {
        public string Name { get; }
        public double TimeStamp { get; }
        public EventType Type { get; }

        public ProfilerEvent(string name, double timeStamp, EventType type)
        {
            (Type, TimeStamp, Name) = (type, timeStamp, name);
        }
    }
}