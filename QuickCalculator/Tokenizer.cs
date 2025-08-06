using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        public static Token[] Tokenize(string input)
        {
            char[] operators = { '+', '-', '*', '/', '^', '%', '!' };
            char[] validSymbols = { '.', ',', '(', ')' };

            Token[] tokens = new Token[input.Length];
            int currentIndex = 0;
            /*  Creates an array of the size of the input. There will usually be fewer tokens than characters,
             *  so there will be some space inefficiency. However, each tokens array is only used once then garbage collected
             *  so this is negligible. It is more space efficient than a linked list and avoids the resizing of a dynamic array.
             */

            char type = '\0';  // \0 is a placeholder character to indicate a type hasn't been determined yet
            string token = "";
            char current;
            bool hasDecimal = false;
            for (int i = 0; i < input.Length; i++)
            {
                current = input[i];

                if (current.Equals(' ')) { // Whitespace is ignored entirely-- even for symbol names and numbers
                    continue;
                }
                else if (!(Char.IsLetter(current) || Char.IsDigit(current) || validSymbols.Contains(current) || operators.Contains(current)))
                { // Only letters, digits, and select symbols are valid
                    throw new TokenizerException("Invalid character '" + current + "' at index "+i + ", unable to tokenize.",i);
                }

                switch (type)
                {
                    case '\0': // If a type hasn't yet been determined, we need to start with 
                        token += current;

                        // If the token starts with a letter, then it is a Variable or Function, collectively called a Symbol
                        if (Char.IsLetter(current))
                        {
                            type = 's'; // s indicates Symbol
                            continue;
                        }

                        // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
                        else if (Char.IsDigit(current) || current == '.')
                        {
                            type = 'n'; // n indicates Number
                            hasDecimal = current == '.';
                            continue;
                        }

                        else if (operators.Contains(current))
                        {
                            type = 'o'; // o indicates Operator
                            /*  Since operators can only be one character long, the token node can immediately without more looping,
                                so we will immediately break out of the switch */
                        }
                        else if (current == '(')
                        {
                            type = '(';
                        }
                        else if (current == ')')
                        {
                            type = ')';
                        }
                        else
                        {
                            // Any other character is invalid for the start of a token
                            throw new TokenizerException("Invalid character '" + current + "' for start of token at index " + i + ", unable to tokenize.", i);
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
                                throw new TokenizerException("Input cannot have multiple decimals, unable to tokenize.", i);
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
                tokens[currentIndex] = new Token(token, type);
                currentIndex++;
                type = '\0'; // Change the type to indicate that we are starting a new token
                token = "";
            }
            tokens[currentIndex] = new Token(token, type); // Insert the last token
            return tokens;
        }
    }
}
