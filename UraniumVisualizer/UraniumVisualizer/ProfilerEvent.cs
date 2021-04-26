namespace UraniumVisualizer
{
    public class ProfilerEvent
    {
        public double StartTime { get; }
        public double EndTime { get; }

        public ProfilerEvent(double startTime, double endTime)
        {
            (StartTime, EndTime) = (startTime, endTime);
        }
    }
}