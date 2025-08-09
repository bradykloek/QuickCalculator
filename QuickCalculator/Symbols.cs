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
            {"pi", Math.PI},
            {"e", Math.E }
        };

        public static Hashtable functions = new Hashtable
        {
            {"sqrt", new Function(1, 
                                        x => Math.Pow(x[0], 0.5)    
            )},

            {"log10", new Function(1,
                                        x => Math.Log(x[0],10)
            )},

            {"log", new Function(2,
                                        x => Math.Log(x[0],x[1])
            )},

            {"ln", new Function(1,
                                        x => Math.Log(x[0],Math.E)
            )},

            {"sin", new Function(1,
                                        x => Math.Sin(x[0])
            )},

            {"cos", new Function(1,
                                        x => Math.Cos(x[0])
            )},

            {"tan", new Function(1,
                                        x => Math.Tan(x[0])
            )},

            {"arcsin", new Function(1,
                                        x => Math.Asin(x[0])
            )},

            {"arccos", new Function(1,
                                        x => Math.Acos(x[0])
            )},

            {"arctan", new Function(1,
                                        x => Math.Atan(x[0])
            )},

            {"deg", new Function(1,
                                        x => x[0] * 180 / Math.PI
            )},

            {"rad", new Function(1,
                                        x => x[0] * Math.PI / 180
            )},

            {"abs", new Function(1,
                                        x => Math.Abs(x[0])
            )},

            {"floor", new Function(1,
                                        x => Math.Floor(x[0])
            )},

            {"ceil", new Function(1,
                                        x => Math.Ceiling(x[0])
            )},

            {"round", new Function(1,
                                        x => Math.Round(x[0])
            )},

            {"truncate", new Function(1,
                                        x => Math.Truncate(x[0])
            )},
        };

    }
}
