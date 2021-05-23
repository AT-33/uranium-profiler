using System.Collections.Generic;

namespace UraniumVisualizer
{
    public interface IVisualizer
    {
        double FunctionHeight { get; set; }
        FunctionRectangle? SelectedFunction { get; }
        void OpenSessionFile(string filePath);
        void ResizeScreen(int width, int height);
        IEnumerable<FunctionRectangle> GetRectangles();
        void ApplyZooming(double factor);
        void ApplyMoving(int factorX, int factorY);
        void SelectFunctionUnderCursor(int x, int y);
        void DeselectAll();
    }
}