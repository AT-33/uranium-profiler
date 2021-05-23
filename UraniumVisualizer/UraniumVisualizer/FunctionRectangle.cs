using System.Collections.Generic;
using System.Drawing;

namespace UraniumVisualizer
{
    public readonly struct FunctionRectangle
    {
        public readonly double Width;
        public readonly double Height;

        public readonly double Left;
        public readonly double Top;

        public double Right => Left + Width;
        public double Bottom => Top + Height;

        public readonly FunctionRecord Record;
        
        public FunctionRectangle(double left, double top, double width, double height, FunctionRecord record)
        {
            Record = record;
            Width = width;
            Height = height;
            Left = left;
            Top = top;
        }
    }
}