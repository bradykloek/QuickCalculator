using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    /// <summary>
    /// Represents a token that holds additional information about what level it was found on. This is used for any elements
    /// that need to be nestable, such as parentheses and functions.
    /// </summary>
    internal class LevelToken : Token
    {
        private int level;
        public LevelToken(string token, char category, int start, int end, int level) : base(token, category, start, end)
        {
            this.level = level;
        }

        public int GetLevel()
        {
            return level;
        }
    }
}
