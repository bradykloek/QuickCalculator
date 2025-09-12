using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Represents an individual TokenText of information. Tokenizer parses an input string to convert it into a list of Tokens.
    /// </summary>
    internal class Token
    {
        public string TokenText { get; private set; } // String representing the contents of a single TokenText
        public TokenCategory category { get; set; }

        // Store the indexes of the input string that comprise this TokenText. [start, end)
        public int StartIndex { get; private set; }
        public int EndIndex {get; private set;} 

        public Token(string TokenText, TokenCategory category, int start, int end)
        {
            this.TokenText = TokenText;
            this.category = category;
            StartIndex = start;
            EndIndex = end;
        }

        public override string ToString()
        {
            return TokenText;
        }
    }
}
