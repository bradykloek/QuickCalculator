using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {

        /// <summary>
        /// Converts an input string into a TokenCollection that stores Tokens
        /// </summary>
        /// <param name="input"></param> Input string
        /// <param name="throwExceptions"></param> Boolean indicating whether exceptions should be thrown. 
        /// <returns></returns>
        public static TokenCollection Tokenize(string input, bool throwExceptions)
        {

            char[] operators = { '+', '-', '*', '/', '^', '%', '!', '=' };
            char[] validSymbols = { '.', ',', '(', ')'};

            TokenCollection tokenCollection = new TokenCollection(input.Length);

            if (input.Length == 0)
            {
                return tokenCollection;
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
                    TokenizerError("Invalid character '" + current + "'.",i, throwExceptions, tokenCollection);
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
                                    TokenizerError("Expression contains multiple assignment operators '='.", i, throwExceptions, tokenCollection);
                                }
                                hasAssignment = true;
                            }
                        }
                        else  if (current == '(' || current == ')')
                        {
                            category = current;

                            if (current == '(') tokenCollection.AddToken(new ParenToken(token, category, inputStart, i, parenLevel++));
                            else
                            {
                                if(parenLevel == 0)
                                {
                                    TokenizerError("Unopened closing parentheses.", i, throwExceptions, tokenCollection);
                                }
                                else
                                {
                                    tokenCollection.AddToken(new ParenToken(token, category, inputStart, i, --parenLevel));

                                }
                            }
                            category = '\0'; // Indicate new token will be started
                            token = "";
                            continue;
                        }
                        else
                        {
                            // Any other character is invalid for the start of a token
                            TokenizerError("Invalid character '" + current + "' for start of token.", i, throwExceptions, tokenCollection);
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
                                TokenizerError("Number contains multiple decimal points.", i, throwExceptions, tokenCollection);
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
                tokenCollection.AddToken(new Token(token, category, inputStart, i));
                category = '\0'; // Change the category to indicate that we are starting a new token
                token = "";
            }
            if (category == 's' || category == 'n') // If the last token is a symbol or number, it hasn't been added yet
            {
                tokenCollection.AddToken(new Token(token, category, inputStart, input.Length));
            }
            return tokenCollection;
        }

        /// <summary>
        /// TokenizerError handles any input that aren't valid for tokenization. It either throws an error or stores the 
        /// index that caused the error in errorIndices, depending on how the Tokinize() call is being used.
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index that caused the error
        /// <param name="throwException"></param> Boolean indicated whether an exceptions should be thrown or not
        /// <param name="tokenCollection"></param> TokenCollection object that will store the error indices if !throwException
        /// <exception cref="TokenizerException"></exception>
        private static void TokenizerError(string message, int charIndex, bool throwException, TokenCollection tokenCollection)
        {
            TokenizerException exception = new TokenizerException(message, charIndex);

            if (throwException)
            {
                throw exception;
            }
            else
            {
                tokenCollection.TokenizerError(exception);  // Adds this exception to the list kept within tokenCollection
            }
        }
    }
}
