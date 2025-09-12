using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Represents a TokenText that holds additional information about what level it was found on. This is used for any elements
    /// that need to be nestable, such as parentheses and functions.
    /// </summary>
    internal class LevelToken : Token
    {
        public int Level {  get; private set; }
        public LevelToken(string TokenText, TokenCategory category, int start, int end, int level) : base(TokenText, category, start, end)
        {   /* A negative level indicates inbalance which will be properly handled by the Validator, but the LevelToken should not store
             * this negative value because it will cause an index out of bounds error when we color the input in InputWindow. */
            if (level < 0) this.Level = 0;
            else this.Level = level;
        }
    }
}
