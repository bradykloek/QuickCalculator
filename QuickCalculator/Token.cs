using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    /// <summary>
    /// Represents an individual token of information. Tokenizer parses an input string to convert it into a list of Tokens.
    /// </summary>
    internal class Token
    {
        private string token; // String representing the contents of a single token
        private char category; /* Indicates the category of the 
                                    *      n = Number
                                    *      o = Operator
                                    *      v = Variable
                                    *      f = Function
                                    *      ( = Open Parenthesis
                                    *      ) = Closed Parenthesis
                                    *      q = Quick Calculator controls
                                    */

        private int inputStart, inputEnd; // Store the indexes of the input string that comprise this token. [start, end)

        public Token(string token, char category, int start, int end)
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

        public char GetCategory()
        {
            return category;
        }

        public void SetCategory(char category)
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
