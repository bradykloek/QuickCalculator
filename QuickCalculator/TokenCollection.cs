using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class TokenCollection
    {
        /// <summary>
        /// Represents a collection of Tokens.
        /// This is the result of a call to Tokenizer.
        /// </summary>


        private Token[] tokens;         // Holds all of the Tokens that are created
        private bool[] errorIndices;    // Booleans represent whether each individual character at each index caused an error
        private int tokenCount = 0;     // Stores how many tokens are in the tokens array

        public TokenCollection(int charCount)
        {
            tokenCount = 0;
            tokens = new Token[charCount+1];
            errorIndices = new bool[charCount+1];

            /* Though these are of the same size, they are NOT parallel arrays. 
             *   - tokens has one element per token. This has an UPPER BOUND of charCount, and due to these arrays being used
             *   only once then discarded, the empty space is negligible. It is more efficient to avoid using a dynamic array that
             *   has to perform any resize operations
             *   - errorIndices is meant to be exactly of length charCount, as each boolean corresponds to the character in its index
             */
        }

        public void addToken(Token token)
        {
            if(tokenCount >= tokens.Length)
            {
                throw new IndexOutOfRangeException("Cannot add token. Tokenizer out of bounds.");
            }

            tokens[tokenCount] = token;
            tokenCount++;
        }

        public void modifyError(int index, bool newStatus)
        {
            if(index < 0 || index >= errorIndices.Length)
            {
                throw new IndexOutOfRangeException("Cannot modify errorIndices. Tokenizer out of bounds.");
            }

            errorIndices[index] = newStatus;
        }

        public Token[] getTokens()
        {
            return tokens;
        }

        public bool[] getErrors()
        {
            return errorIndices;
        }

        public int getTokenCount()
        {
            return tokenCount;
        }

    }
}
