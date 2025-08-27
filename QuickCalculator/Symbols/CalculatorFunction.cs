using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickCalculator.Symbols
{
    internal abstract class CalculatorFunction
    {
        public abstract int GetNumParameters();
        public abstract double Execute(List<double> args); 
    }
}
