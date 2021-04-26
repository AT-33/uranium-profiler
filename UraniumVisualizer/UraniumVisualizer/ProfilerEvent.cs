using System;

namespace UraniumVisualizer
{
    public readonly struct ProfilerEvent
    {
        public double TimeStamp { get; }
        public EventType Type { get; }

        public ProfilerEvent(double timeStamp, EventType type)
        {
            (Type, TimeStamp) = (type, timeStamp);
        }
    }
}