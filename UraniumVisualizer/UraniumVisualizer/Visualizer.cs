using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public double LeftSeconds => leftOffset - 0.5 * secondsVisible;
        public double RightSeconds => LeftSeconds + secondsVisible;

        public double LeftPixels => LeftSeconds / SecondsPerPixel;
        public double RightPixels => RightSeconds / SecondsPerPixel;

        private double TopPixels { get; set; }
        private double BottomPixels => TopPixels + screenHeight;

        private List<FunctionRectangle> cachedRectangles;
        private List<FunctionRecord> parsingResults;

        public double SecondsToPixels(double seconds)
        {
            return (seconds - LeftSeconds) / SecondsPerPixel;
        }

        public FunctionRectangle? SelectedFunction { get; private set; }
        public Dictionary<string, (int count, double maxTime)> FunctionData => parser?.FunctionData;

        public void OpenSessionFile(string filePath)
        {
            parser = new FileParser(filePath);
            parsingResults = parser.Parse().ToList();
            secondsVisible = parsingResults.Last().PositionX + parsingResults.Last().Duration
                             - parsingResults.First().PositionX;
            leftOffset = secondsVisible * 0.5;
            if (LeftSeconds < 0)
                leftOffset -= LeftSeconds;
            cachedRectangles = null;
        }

        public void ResizeScreen(int width, int height)
        {
            (screenWidth, screenHeight) = (width, height);
            if (LeftSeconds < 0)
                leftOffset -= LeftSeconds;
        }

        public IEnumerable<FunctionRectangle> GetRectangles()
        {
            cachedRectangles = cachedRectangles ?? UpdateRectangles().ToList();

            return cachedRectangles
                .Where(f => f.Width > 1)
                .Where(f => f.Left <= RightPixels || f.Right >= LeftPixels)
                .Where(f => f.Top <= BottomPixels || f.Bottom >= TopPixels);
        }

        public void ApplyZooming(double factor)
        {
            secondsVisible *= factor;
            if (LeftSeconds < 0)
                leftOffset -= LeftSeconds;
            cachedRectangles = null;
        }

        public void ApplyMoving(int factorX, int factorY)
        {
            leftOffset -= factorX * SecondsPerPixel;
            if (LeftSeconds < 0)
                leftOffset -= LeftSeconds;
            cachedRectangles = null;
        }

        public void SelectFunctionUnderCursor(int x, int y)
        {
            SelectedFunction = cachedRectangles
                .AsParallel()
                .FirstOrDefault(r => x >= r.Left && x <= r.Right && y >= r.Top && y <= r.Bottom);
        }

        public void DeselectAll()
        {
            SelectedFunction = null;
        }

        public List<FunctionRecord> GetAllSubFunctions(FunctionRecord record)
        {
            var parent = null as FunctionRectangle?;
            var result = new List<FunctionRecord>();
            foreach (var rectangle in (cachedRectangles as IEnumerable<FunctionRectangle>).Reverse())
            {
                if (rectangle.Record == record)
                    parent = rectangle;
                if (!parent.HasValue) continue;
                if (rectangle.Left >= parent.Value.Left)
                    result.Add(rectangle.Record);
                else
                    break;
            }

            return result;
        }

        private IEnumerable<FunctionRectangle> UpdateRectangles()
        {
            if (SelectedFunction?.Record != null)
                SelectedFunction = TransformFunction(SelectedFunction.Value.Record);
            if (parsingResults is null)
                yield break;

            foreach (var rectangle in parsingResults)
            {
                yield return TransformFunction(rectangle);
            }
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