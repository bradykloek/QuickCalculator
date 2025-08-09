using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Symbols
    {
        public static Hashtable variables = new Hashtable
        {
            {"pi", 3.14159265}
        };

        public static Hashtable functions = new Hashtable
        {
            {"square", new Function(1, 
                                        x => x[0] * x[0]    
            )},

                        {"mult", new Function(2,
                                        x => x[0] * x[1] + 1
            )}
        };

    }
}
