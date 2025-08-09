using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class FunctionToken : LevelToken
    {
        private int level;
        private List<double> arguments;
        public FunctionToken(string token, char category, int start, int end, int level) : base(token, category, start, end, level)
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

        public int GetLevel()
        {
            return level;
        }
    }
}