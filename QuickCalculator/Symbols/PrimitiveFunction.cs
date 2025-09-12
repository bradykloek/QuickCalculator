using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Symbols
{
    internal class PrimitiveFunction : CalcFunction
    {
        private Func<List<double>, double> function;
        private int numParameters;
        public PrimitiveFunction(int numParameters, Func<List<double>, double> function)
        {
            this.numParameters = numParameters;
            this.function = function;
        }

        public override double Execute(List<double> args)
        {
            return function(args);
        }

        public override int NumParameters()
        {
            return numParameters;
        }
    }
}
