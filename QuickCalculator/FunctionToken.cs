using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class FunctionToken : Token
    {
        private int level;
        public FunctionToken(string token, char category, int start, int end, int level) : base(token, category, start, end)
        {
            this.level = level;
        }

        public int GetLevel()
        {
            return level;
        }
    }
}