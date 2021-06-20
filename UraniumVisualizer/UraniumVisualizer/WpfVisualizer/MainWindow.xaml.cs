using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using UraniumVisualizer;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace WpfVisualizer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string CurrentFileName { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void VisualizerForm_OnSelectionChanged(List<FunctionRecord> records,
            Dictionary<string,(int count, double maxTime)> functionData)
        {
            var functions = records
                .Select(r => ConvertFunctionRecord(functionData, r))
                .ToArray();
            FunctionTreeView.ItemsSource = records.Any()
                ? new List<FunctionNode> {ProcessFunction(records, functionData, 0)}
                : Enumerable.Empty<FunctionNode>();
            
            Functions.ItemsSource = functions;
        }

        private static FunctionInfo ConvertFunctionRecord(Dictionary<string, (int count, double maxTime)> functionData,
            FunctionRecord record)
        {
            return new FunctionInfo(record.Name, record.Duration * 1000)
            {
                SelfMs = record.SelfDuration * 1000,
                MaxMs = functionData[record.Name].maxTime * 1000,
                Count = functionData[record.Name].count,
                CallstackIndex = record.PositionY
            };
        }

        private static FunctionNode ProcessFunction(List<FunctionRecord> records,
            Dictionary<string, (int count, double maxTime)> functionData, int index)
        {
            var record = records[index];
            var children = records
                .Zip(Enumerable.Range(0, int.MaxValue), (f, i) => (f, i))
                .Where(r => r.f.PositionX >= record.PositionX)
                .Where(r => r.f.PositionY == record.PositionY + 1)
                .Select(r => ProcessFunction(records, functionData, r.i));
            return new FunctionNode {Children = children, Info = ConvertFunctionRecord(functionData, record)};
        }

        private void OpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(CurrentFileName) ?? Environment.CurrentDirectory,
                Multiselect = false,
                Filter = "Uranium profiler records (*.ups)|*.ups|All files (*.*)|*.*"
            };
            if(openFileDialog.ShowDialog() == true && Host.Child is VisualizerForm form)
            {
                CurrentFileName = openFileDialog.FileName;
                form.OpenSessionFile(CurrentFileName);
            }
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Reload_OnClick(object sender, RoutedEventArgs e)
        {
            if (Host.Child is VisualizerForm form)
                form.OpenSessionFile(CurrentFileName);
        }
    }
}