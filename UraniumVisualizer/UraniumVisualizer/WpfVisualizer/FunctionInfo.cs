using System;
using System.Collections.Generic;

namespace WpfVisualizer
{
    public class FunctionInfo
    {
        public double TotalMs { get; }

        public double SelfMs { get; set; }

        public string Name { get; }

        public int Count { get; set; }

        public double MaxMs { get; set; }

        public int CallstackIndex { get; set; }

        public double TotalWidth => ChildWidth + SelfWidth;
        public double SelfWidth => SelfPercent * 3;
        public double SelfPercent => SelfMs / TotalMs * 100;
        public double ChildWidth => ChildPercent * 3;
        private double ChildPercent => 100 - SelfPercent;

        public FunctionInfo(string name, double totalMs)
        {
            Name = name;
            TotalMs = totalMs;
        }
    }
}