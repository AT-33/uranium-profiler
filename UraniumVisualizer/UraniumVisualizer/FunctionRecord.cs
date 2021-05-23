namespace UraniumVisualizer
{
    public class FunctionRecord
    {
        public string Name { get; }
        public int PositionY { get; }
        public double PositionX { get; }
        public double Duration { get; }
        
        public FunctionRecord(string name, int y, double x, double d)
        {
            Name = name;
            PositionY = y;
            PositionX = x / 1_000_000;
            Duration = d / 1_000_000;
        }
    }
}
