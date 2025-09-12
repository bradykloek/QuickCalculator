using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Token that represents a Function. Has an additional field, arguments, that is a List that holds 
    /// the arguments passed into the function as doubles. The Parser will parse the argument Tokens first to
    /// populate this list before the function is executed.
    /// </summary>
    internal class FunctionToken : LevelToken
    {
        private List<double> arguments;
        public FunctionToken(string TokenText, TokenCategory category, int start, int end, int level) : base(TokenText, category, start, end, level)
        {
            arguments = new List<double>();
        }

        public void AddArg(double arg)
        {
            arguments.Add(arg);
        }
    }
}