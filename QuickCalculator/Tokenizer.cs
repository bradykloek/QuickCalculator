using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        private string input;
        private bool throwExceptions;
        private Token[] tokens;         // Holds all of the Tokens that are created
        private List<TokenizerException> tokenizerExceptions;    // List that stores all exceptions that were encountered
        private int tokenCount = 0;     // Stores how many tokens are in the tokens array

        public Tokenizer(string input, bool throwExceptions)
        {
            this.input = input;
            this.throwExceptions = throwExceptions;
            tokenCount = 0;
            tokens = new Token[input.Length + 1];
            tokenizerExceptions = new List<TokenizerException>();

            Tokenize();
        }


        public void AddToken(Token token)
        {
            if (tokenCount >= tokens.Length)
            {
                throw new IndexOutOfRangeException("Cannot add token. Tokenizer out of bounds.");
            }

            tokens[tokenCount] = token;
            tokenCount++;
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

        /// <summary>
        /// Converts an input string into a TokenCollection that stores Tokens
        /// </summary>
        /// <param name="input"></param> Input string
        /// <param name="throwExceptions"></param> Boolean indicating whether exceptions should be thrown. 
        /// <returns></returns>
        private void Tokenize()
        {

            char[] operators = { '+', '-', '*', '/', '^', '%', '!', '=' };
            char[] validSymbols = { '.', ',', '(', ')'};

            if (input.Length == 0)
            {
                return;
            }


            bool hasDecimal = false;
            bool hasAssignment = false;
            int parenLevel = 0;
            int inputStart = 0;

            char category = '\0';  // \0 is a placeholder character to indicate a category hasn't been determined yet
            string token = "";
            char current;
            for (int i = 0; i < input.Length; i++)
            {
                current = input[i];

                if (current.Equals(' ')) { // Whitespace is ignored entirely-- even for symbol names and numbers
                    continue;
                }
                else if (!(Char.IsLetter(current) || Char.IsDigit(current) || validSymbols.Contains(current) || operators.Contains(current)))
                { // Only letters, digits, and select symbols are valid
                    TokenizerError("Invalid character '" + current + "'.",i, throwExceptions);
                    continue;
                }

                switch (category)
                {
                    case '\0': // If a category hasn't yet been determined, we need to start with 
                        token += current;
                        inputStart = i;

                        // If the token starts with a letter, then it is a Variable or Function, collectively called a Symbol
                        if (Char.IsLetter(current))
                        {
                            category = 's'; // s indicates Symbol
                            continue;
                        }

                        // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
                        else if (Char.IsDigit(current) || current == '.')
                        {
                            category = 'n'; // n indicates Number
                            hasDecimal = current == '.';
                            continue;
                        }

                        else if (operators.Contains(current))
                        {
                            category = 'o'; // o indicates Operator
                            /*  Since operators can only be one character long, the token node can immediately without more looping,
                                so we will immediately break out of the switch */

                            if(current == '=')
                            {
                                if (hasAssignment)
                                {
                                    TokenizerError("Expression contains multiple assignment operators '='.", i, throwExceptions);
                                }
                                hasAssignment = true;
                            }
                        }
                        else  if (current == '(' || current == ')')
                        {
                            category = current;

                            if (current == '(') AddToken(new ParenToken(token, category, inputStart, i, parenLevel++));
                            else
                            {
                                if(parenLevel == 0)
                                {
                                    TokenizerError("Unopened closing parenthesis.", i, throwExceptions);
                                }
                                else
                                {
                                    AddToken(new ParenToken(token, category, inputStart, i, --parenLevel));

                                }
                            }
                            category = '\0'; // Indicate new token will be started
                            token = "";
                            continue;
                        }
                        else
                        {
                            // Any other character is invalid for the start of a token
                            TokenizerError("Invalid character '" + current + "' for start of token.", i, throwExceptions);
                        }
                            break;
                    case 's':
                        // This character continues the token if it is a letter or digit
                        if (Char.IsLetter(current) || Char.IsDigit(current))
                        {
                            token += current;
                            continue;
                        }
                        /*  If not, then the current token ends and we will start a new one.
                            *  We must move the counter (i) back so the loop will check the current character again */
                        i--;
                        break;
                    case 'n':
                        // This character continues the token only if it is a digit or decimal
                        if (Char.IsDigit(current))
                        {
                            token += current;
                            continue;
                        }
                        else if(current == ',')
                        {
                            continue; // Commas can be in numbers and are simply ignored
                        }
                        else if (current == '.')
                        {
                            if (hasDecimal) {
                                TokenizerError("Number contains multiple decimal points.", i, throwExceptions);
                            }
                            else {
                                hasDecimal = true;
                                token += current;
                                continue;
                            }
                        }
                        else
                        {
                        /*  If none of the above, then the current token ends and we will start a new one.
                        *  We must move the counter (i) back so the loop will check the current character again */
                            i--;
                        }
                        break;
                }
                AddToken(new Token(token, category, inputStart, i));
                category = '\0'; // Change the category to indicate that we are starting a new token
                token = "";
            }
            if (category == 's' || category == 'n') // If the last token is a symbol or number, it hasn't been added yet
            {
                AddToken(new Token(token, category, inputStart, input.Length));
            }

            // Check if all parentheses have been matched
            if(parenLevel != 0)
            {
                Token[] tokens = GetTokens();
                for (int i = 0; i < parenLevel; i++)
                {
                    for (int idx = 0; idx < GetTokenCount(); idx++)
                    {
                        if (tokens[idx].GetCategory() == '(')
                        {
                            TokenizerError("Unmatched open parenthesis.", tokens[idx].GetStart(), throwExceptions);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// TokenizerError handles any input that aren't valid for tokenization. It either throws an error or stores the 
        /// index that caused the error in errorIndices, depending on how the Tokinize() call is being used.
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index that caused the error
        /// <param name="throwException"></param> Boolean indicated whether an exceptions should be thrown or not
        /// <exception cref="TokenizerException"></exception>
        private void TokenizerError(string message, int charIndex, bool throwException)
        {
            TokenizerException exception = new TokenizerException(message, charIndex);

            if (throwException)
            {
                throw exception;
            }
            else
            {
                tokenizerExceptions.Add(exception);
            }
        }
    }
}
