using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        public static LinkedList Tokenize(string input)
        {
            char[] operators = { '+', '-', '*', '/', '^', '%', '!' };

            LinkedList tokens = new LinkedList();
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
                else if (!(Char.IsLetter(current) || Char.IsDigit(current) || current == '.' || operators.Contains(current)))
                { // Only letters, digits, and operators are valid
                    throw new Exception("Invalid character, unable to tokenize.");
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

                            // Otherwise it must be an operator
                            else 
                            {
                                type = 'o'; // o indicates Operator
                                /*  Since operators can only be one character long, the token node can immediately without more looping,
                                    so we will immediately break out of the switch */
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
                            if (Char.IsDigit(current) || current == '.')
                            {
                                if (current == '.')
                                {
                                    if (hasDecimal) {
                                        throw new Exception("Number cannot have multiple decimals");
                                    }
                                    else {
                                        hasDecimal = true;
                                    }
                                }
                                token += current;
                                continue;
                            }
                            /*  If not, then the current token ends and we will start a new one.
                             *  We must move the counter (i) back so the loop will check the current character again */
                            i--;
                            break;
                    }
                tokens.Insert(new Node(token, type));
                type = '\0'; // Change the type to indicate that we are starting a new token
                token = "";
            }
            tokens.Insert(new Node(token, type)); // Insert the last token
            return tokens;
        }
    }
}
