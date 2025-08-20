using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    /// <summary>
    /// Stores all of the defined variables and functions in Hashtables.
    /// The global symbols are held in static Hashtables.
    /// Creating an instance of SymbolTable provides an instance variable localVariables that represents
    /// a single local scope of variables.
    /// </summary>
    internal class SymbolTable
    {
        public static Hashtable variables = new Hashtable
        {
            {"ans", new Variable(0) },
            {"pi", new Variable(Double.Pi)},
            {"e", new Variable(Double.E)},
            {"inf", new Variable(Double.PositiveInfinity)}
        };

        public static Hashtable functions = new Hashtable
        {
            {"sqrt", new PrimitiveFunction(1,
                                        x => Math.Pow(x[0], 0.5)
            )},

            {"log10", new PrimitiveFunction(1,
                                        x => Math.Log(x[0],10)
            )},

            {"log", new PrimitiveFunction(2,
                                        x => Math.Log(x[0],x[1])
            )},

            {"ln", new PrimitiveFunction(1,
                                        x => Math.Log(x[0],Math.E)
            )},

            {"sin", new PrimitiveFunction(1,
                                        x => Math.Sin(x[0])
            )},

            {"cos", new PrimitiveFunction(1,
                                        x => Math.Cos(x[0])
            )},

            {"tan", new PrimitiveFunction(1,
                                        x => Math.Tan(x[0])
            )},

            {"arcsin", new PrimitiveFunction(1,
                                        x => Math.Asin(x[0])
            )},

            {"arccos", new PrimitiveFunction(1,
                                        x => Math.Acos(x[0])
            )},

            {"arctan", new PrimitiveFunction(1,
                                        x => Math.Atan(x[0])
            )},

            {"deg", new PrimitiveFunction(1,
                                        x => x[0] * 180 / Math.PI
            )},

            {"rad", new PrimitiveFunction(1,
                                        x => x[0] * Math.PI / 180
            )},

            {"abs", new PrimitiveFunction(1,
                                        x => Math.Abs(x[0])
            )},

            {"floor", new PrimitiveFunction(1,
                                        x => Math.Floor(x[0])
            )},

            {"ceil", new PrimitiveFunction(1,
                                        x => Math.Ceiling(x[0])
            )},

            {"round", new PrimitiveFunction(1,
                                        x => Math.Round(x[0])
            )},

            {"truncate", new PrimitiveFunction(1,
                                        x => Math.Truncate(x[0])
            )},

            {"factorial", new PrimitiveFunction(1,
                                        x => {
                                            if(x[0] < 0) return double.NaN;
                                            double result = 1;
                                            for(int i = 1; i <= x[0]; i++)
                                            {
                                                result *= i;
                                            }
                                            return result;
                                        }
            )},

            {"random", new PrimitiveFunction(0,
                                        x => {
                                            Random random = new Random();
                                            return random.NextDouble();
                                        }
            )},

            {"randomInt", new PrimitiveFunction(2,
                                        x => {
                                            Random random = new Random();
                                            return random.Next((int)x[0],(int)x[1]+1);
                                        }
            )},
        };

        private Hashtable localVariables;

        public SymbolTable()
        {
            localVariables = new Hashtable();
        }

        public void AddLocal(string variable, double value)
        {
            localVariables[variable] = value;
        }

        public bool LocalsContains(object o)
        {
            return localVariables.ContainsKey(o);
        }

        public double GetLocal(string variable)
        {
            return (double)localVariables[variable];
        }
    }
}
