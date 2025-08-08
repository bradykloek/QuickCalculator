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
        private List<Token> tokens;                 // Stores the tokens that get created
        private List<EvaluationException> exceptions;       // If we don't throw exceptions, they will be stored here
        private Evaluator evaluator;

        public Tokenizer(string input, Evaluator evaluator)
        {
            this.input = input;
            this.evaluator = evaluator;
            tokens = new List<Token>(input.Length / 2);
            // Starts tokens with a capacity of 50% of input.Length to avoid the initial resizings which would almost always occur

            Tokenize();
        }


        public List<Token> GetTokens()
        {
            return tokens;
        }

        /// <summary>
        /// Takes in the input string to populate the tokens array.
        /// If !throwExceptions, it will also fill the exceptions list.
        /// This is called from the constructor.
        /// </summary>
        private void Tokenize()
        {

            char[] operators = { '+', '-', '*', '/', '^', '%', '!', '=' };
            char[] validSymbols = { '.', ',', '(', ')', '[', ']'};

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
                bool implicitMultiplication = false;

                if (current.Equals(' ')) { // Whitespace is ignored entirely-- even for symbol names and numbers
                    continue;
                }
                else if (!(Char.IsLetter(current) || Char.IsDigit(current) || validSymbols.Contains(current) || operators.Contains(current)))
                { // Only letters, digits, and select symbols are valid
                    evaluator.AddException("Invalid character '" + current + "'.",i,i+1);
                    continue;
                }

                switch (category)
                {
                    case '\0': // If a category hasn't yet been determined, we need to start with 
                        token += current;
                        inputStart = i;

                        /* If the token starts with a letter, then it is a Variable or Function. It will be marked as a variable
                         * and stay as such unless a [ is found indicating it is a function */
                        if (Char.IsLetter(current))
                        {
                            category = 'v'; // v indicates Variable
                            continue;
                        }

                        // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
                        else if (Char.IsDigit(current) || current == '.')
                        {
                            category = 'n'; // n indicates Number
                            hasDecimal = current == '.';
                            continue;
                        }

                        // Before checking for operators, we first must handle a special case for - to check
                        // if it should be the unary negation operator instead of subtraction
                        else if (current == '-')
                        {
                            // If this is the first token or the previous token was an operator, it is unary negation
                            if(tokens.Count() == 0 || tokens[tokens.Count() - 1].GetCategory() == 'o')
                            {
                                category = 'n';
                                continue;
                            }
                        }

                        else if (operators.Contains(current))
                        {
                            category = 'o'; // o indicates Operator
                            /*  Since operators can only be one character long, the token node can immediately without more looping,
                                so we will immediately break out of the switch */

                            if (current == '=')
                            {
                                if (hasAssignment)
                                {
                                    evaluator.AddException("Expression contains multiple assignment operators '='.", i, i+1);
                                }
                                hasAssignment = true;
                            }

                            // The only operator that is more than one character is // for integer division. Special case.
                            if (current == '/')
                            {
                                if (i + 1 < input.Length && input[i + 1] == '/')
                                {
                                    token = "//";
                                    i++; // Move past the second '/' since we have already tokenized it
                                }
                            }
                        }
                        else if (current == '(' || current == ')')
                        {
                            category = current;

                            if (current == '(') tokens.Add(new ParenToken(token, category, inputStart, i, parenLevel++));
                            else
                            {
                                if (parenLevel == 0)
                                {
                                    evaluator.AddException("Unmatched closing parenthesis.", i, i + 1);
                                }
                                else
                                {
                                    tokens.Add(new ParenToken(token, category, inputStart, i, --parenLevel));

                                }
                            }
                            category = '\0'; // Indicate new token will be started
                            token = "";
                            continue;
                        }
                        else
                        {
                            // Any other character is invalid for the start of a token
                            evaluator.AddException("Invalid character '" + current + "' for start of token.", i, i + 1);
                        }
                            break;
                    case 'v':
                        // This character continues the token if it is a letter or digit
                        if (Char.IsLetter(current) || Char.IsDigit(current))
                        {
                            token += current;
                            continue;
                        }

                        if(current == '[')
                        {
                            category = 'f'; // If we encounter a [, this indicates this variable is actually a function
                        }

                        /*  The current token ends and we will start a new one.
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
                        else if (current == ',')
                        {
                            continue; // Commas can be in numbers and are simply ignored
                        }
                        else if (current == '.')
                        {
                            if (hasDecimal)
                            {
                                evaluator.AddException("Number contains multiple decimal points.", i, i + 1);
                            }
                            else
                            {
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

                            if (current == '(')
                            {
                                /*  If we encounter a parenthesis immediately following the number token, it is interpreted
                                *   as implicit multiplication */
                                implicitMultiplication = true;
                            }
                        }
                        break;
                }
                tokens.Add(new Token(token, category, inputStart, i + 1));
                if (implicitMultiplication)
                {
                    // If implicit multiplication was detected, add a * token (of length 0)
                    tokens.Add(new Token("*", 'o', i, i));
                }
                category = '\0'; // Change the category to indicate that we are starting a new token
                token = "";
            }
            if (category == 'v' || category == 'n' || category == 'f') // If the last token is a symbol, number, or function, it hasn't been added yet
            {
                tokens.Add(new Token(token, category, inputStart, input.Length));
            }

            // Check if all parentheses have been matched
            if(parenLevel != 0)
            {
                checkParentheses();
            }
        }

        /// <summary>
        /// Finds which open parentheses tokens are unmatched by a closing parentheses at the same level.
        /// This method is only called after it is known that there is at least one unmatched parenthesis
        /// </summary>
        private void checkParentheses()
        {
            for(int i = 0; i < tokens.Count(); i++)
            {
                bool matched = false;
                if (tokens[i].GetCategory() == '(')
                {
                    ParenToken openParen = (ParenToken)tokens[i];
                    for(int j = i+1; j < tokens.Count(); j++)
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
                        evaluator.AddException("Unmatched open parenthesis.", openParen.GetStart(), openParen.GetEnd());
                    }

                }
            }
        }
    }
}
