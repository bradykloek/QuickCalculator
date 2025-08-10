using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickCalculator
{
    internal abstract class Function
    {
        public abstract int GetNumArgs();

        public abstract double Execute(List<double> args); 
    }
}
