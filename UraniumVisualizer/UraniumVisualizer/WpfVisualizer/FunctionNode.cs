using System.Collections.Generic;

namespace WpfVisualizer
{
    public class FunctionNode
    {
        public FunctionInfo Info { get; set; }
        public IEnumerable<FunctionNode> Children { get; set; }
    }
}