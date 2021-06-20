using System.Collections.Generic;

namespace UraniumVisualizer
{
    public interface IVisualizer
    {
        double FunctionHeight { get; set; }
        
        double LeftSeconds { get; }
        double RightSeconds { get; }
        double LeftPixels { get; }
        double RightPixels { get; }

        double SecondsToPixels(double seconds);
        
        FunctionRectangle? SelectedFunction { get; }
        Dictionary<string,(int count, double maxTime)> FunctionData { get; }
        void OpenSessionFile(string filePath);
        void ResizeScreen(int width, int height);
        IEnumerable<FunctionRectangle> GetRectangles();
        void ApplyZooming(double factor);
        void ApplyMoving(int factorX, int factorY);
        void SelectFunctionUnderCursor(int x, int y);
        void DeselectAll();
        List<FunctionRecord> GetAllSubFunctions(FunctionRecord record);
    }
}