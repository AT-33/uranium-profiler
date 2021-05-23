using System;
using System.Collections.Generic;
using System.Linq;

namespace UraniumVisualizer
{
    public class Visualizer : IVisualizer
    {
        public double FunctionHeight { get; set; }

        private FileParser parser;
        private double secondsVisible = 1;
        private double leftOffset = 0.5;

        private int screenWidth;
        private int screenHeight;

        private double SecondsPerPixel => secondsVisible / screenWidth;

        private double LeftSeconds => leftOffset - 0.5 * secondsVisible;
        private double RightSeconds => LeftSeconds + secondsVisible;

        private double LeftPixels => LeftSeconds / SecondsPerPixel;
        private double RightPixels => RightSeconds / SecondsPerPixel;

        private double TopPixels { get; set; }
        private double BottomPixels => TopPixels + screenHeight;

        private List<FunctionRectangle> cachedRectangles;
        private List<FunctionRecord> parsingResults;

        public FunctionRectangle? SelectedFunction { get; private set; }

        public void OpenSessionFile(string filePath)
        {
            parser = new FileParser(filePath);
            parsingResults = parser.Parse().ToList();
        }

        public void ResizeScreen(int width, int height)
        {
            (screenWidth, screenHeight) = (width, height);
        }

        public IEnumerable<FunctionRectangle> GetRectangles()
        {
            cachedRectangles = cachedRectangles ?? UpdateRectangles();
            return cachedRectangles
                .Where(r => r.Left <= RightPixels || r.Right >= LeftPixels)
                .Where(r => r.Top <= BottomPixels || r.Bottom >= TopPixels);
        }

        public void ApplyZooming(double factor)
        {
            secondsVisible = factor < 0
                ? secondsVisible * -factor
                : secondsVisible / factor;
            cachedRectangles = null;
        }

        public void ApplyMoving(int factorX, int factorY)
        {
            leftOffset -= factorX * SecondsPerPixel;
            cachedRectangles = null;
        }

        public void SelectFunctionUnderCursor(int x, int y)
        {
            SelectedFunction = cachedRectangles
                .FirstOrDefault(r => x >= r.Left && x <= r.Right && y >= r.Top && y <= r.Bottom);
        }

        public void DeselectAll()
        {
            SelectedFunction = null;
        }

        private List<FunctionRectangle> UpdateRectangles()
        {
            if (SelectedFunction != null)
                SelectedFunction = TransformFunction(SelectedFunction.Value.Record);
            return parsingResults
                .Select(TransformFunction)
                .ToList();
        }

        private FunctionRectangle TransformFunction(FunctionRecord r)
        {
            return new FunctionRectangle(r.PositionX / SecondsPerPixel - LeftPixels,
                r.PositionY * FunctionHeight,
                r.Duration / SecondsPerPixel,
                FunctionHeight, r);
        }
    }
}