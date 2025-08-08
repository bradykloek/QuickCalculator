using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Function
    {
        private int numArgs;
        private Func<List<double>, double> function;

        public Function(int args, Func<List<double>, double> func)
        {
            numArgs = args;
            function = func;
        }

        public int GetNumArgs()
        {
            return numArgs;
        }

        public double Execute(List<double> args)
        {
            return function(args);
        }
    }
}
