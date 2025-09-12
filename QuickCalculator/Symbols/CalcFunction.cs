using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickCalculator.Symbols
{
    internal abstract class CalcFunction
    {
        public abstract double Execute(List<double> args);
        public abstract int NumParameters();
    }
}
