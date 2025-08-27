using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Represents an individual token of information. Tokenizer parses an input string to convert it into a list of Tokens.
    /// </summary>
    internal class Token
    {
        private string token; // String representing the contents of a single token
        private TokenCategory category;

        private int inputStart, inputEnd; // Store the indexes of the input string that comprise this token. [start, end)

        public Token(string token, TokenCategory category, int start, int end)
        {
            this.token = token;
            this.category = category;
            inputStart = start;
            inputEnd = end;
        }

        public override string ToString()
        {
            return token;
        }

        public string GetToken()
        {
            return token;
        }

        public TokenCategory GetCategory()
        {
            return category;
        }

        public void SetCategory(TokenCategory category)
        {
            this.category = category;
        }

        public int GetStart()
        {
            return inputStart;
        }

        public int GetEnd()
        {
            return inputEnd;
        }
    }
}
