using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Token that represents a Function. Has an additional field, arguments, that is a List that holds 
    /// the arguments passed into the function as doubles. The Parser will parse the argument tokens first to
    /// populate this list before the function is executed.
    /// </summary>
    internal class FunctionToken : LevelToken
    {
        private int level;
        private List<double> arguments;
        public FunctionToken(string token, TokenCategory category, int start, int end, int level) : base(token, category, start, end, level)
        {
            this.level = level;
            arguments = new List<double>();
        }

        public List<double> GetArgs()
        {
            return arguments;
        }

        public void AddArg(double arg)
        {
            arguments.Add(arg);
        }

        public int GetNumArgs()
        {
            return arguments.Count;
        }
    }
}