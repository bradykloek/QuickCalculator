using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class ParenToken : Token
    {
        private int level;
        public ParenToken(string token, char type, int level) : base(token, type)
        {
            this.level = level;
        }

        public int GetLevel()
        {
            return level;
        }
    }
}
