using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        private string input;                   // The raw string input
        private bool throwException;            // Whether we should throw exceptions or just store them
        private Token[] tokens;                 // Stores the tokens that get created
        private int tokenCount = 0;             // Number of tokens that get created
        private List<TokenizerException> tokenizerExceptions;       // If we don't throw exceptions, they will be stored here

        public Tokenizer(string input, bool throwException)
        {
            this.input = input;
            this.throwException = throwException;
            tokenCount = 0;
            tokens = new Token[input.Length + 1];
            tokenizerExceptions = new List<TokenizerException>();

            Tokenize();
        }

        /// <summary>
        /// Adds a new token to the tokens array
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
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
        /// Takes in the input string to populate the tokens array.
        /// If !throwExceptions, it will also fill the tokenizerExceptions list.
        /// This is called from the constructor.
        /// </summary>
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
                    TokenizerError("Invalid character '" + current + "'.",i);
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
                                    TokenizerError("Expression contains multiple assignment operators '='.", i);
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
                                    TokenizerError("Unopened closing parenthesis.", i);
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
                            TokenizerError("Invalid character '" + current + "' for start of token.", i);
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
                                TokenizerError("Number contains multiple decimal points.", i);
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
                checkParentheses();
            }
        }

        /// <summary>
        /// TokenizerError handles any input that aren't valid for tokenization. It either throws an error or stores the 
        /// index that caused the error in errorIndices, depending on how the Tokinize() call is being used.
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index of the input string that caused the error
        /// <exception cref="TokenizerException"></exception>
        private void TokenizerError(string message, int charIndex)
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

        /// <summary>
        /// Finds which open parentheses tokens are unmatched by a closing parentheses at the same level.
        /// This method is only called after it is known that there is at least one unmatched parenthesis
        /// </summary>
        private void checkParentheses()
        {
            for(int i = 0; i < tokenCount; i++)
            {
                bool matched = false;
                if (tokens[i].GetCategory() == '(')
                {
                    ParenToken openParen = (ParenToken)tokens[i];
                    for(int j = i+1; j < tokenCount; j++)
                    {
                        if (tokens[j].GetCategory() == ')')
                        {
                            ParenToken closeParen = (ParenToken)tokens[j];
                            if(openParen.GetLevel() == closeParen.GetLevel())
                            {
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        TokenizerError("Unmatched open parenthesis.", openParen.GetStart());
                    }

                }
            }
        }
    }
}
