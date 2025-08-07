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
        private List<TokenizerException> tokenizerExceptions;    // List that stores all exceptions that were encountered
        private int tokenCount = 0;     // Stores how many tokens are in the tokens array

        public TokenCollection(int charCount)
        {
            tokenCount = 0;
            tokens = new Token[charCount+1];
            tokenizerExceptions = new List<TokenizerException>();

            /*   tokens has one element per token. This has an UPPER BOUND of charCount, and due to these arrays being used
             *   only once then discarded, the empty space is negligible. It is more efficient to avoid using a dynamic array that
             *   has to perform any resize operations
             */
        }

        public void AddToken(Token token)
        {
            if(tokenCount >= tokens.Length)
            {
                throw new IndexOutOfRangeException("Cannot add token. Tokenizer out of bounds.");
            }

            tokens[tokenCount] = token;
            tokenCount++;
        }

        public void TokenizerError(TokenizerException exception)
        {
            tokenizerExceptions.Add(exception);
        }

        public Token[] GetTokens()
        {
            return tokens;
        }

        public List<TokenizerException> GetExceptions()
        {
            return tokenizerExceptions;
        }

        public int GetTokenCount()
        {
            return tokenCount;
        }
    }
}
