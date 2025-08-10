using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class PrimitiveFunction : Function
    {
        private Func<List<double>, double> function;
        private int numArgs;

        public PrimitiveFunction(int numArgs, Func<List<double>, double> function)
        {
            this.numArgs = numArgs;
            this.function = function;
        }

        public override int GetNumArgs()
        {
            return numArgs;
        }

        public override double Execute(List<double> args)
        {
            return function(args);
        }
    }
}
