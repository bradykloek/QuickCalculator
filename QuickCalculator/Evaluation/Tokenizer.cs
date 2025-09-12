using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Tokenizer
    {
        private string input;                       // The raw string input
        private List<Token> Tokens;                 // Stores the Tokens that get created


        private static char[] operators = { '+', '-', '*', '/', '^', '%', '!'};

        // Flags and variables that store temporary information for the Tokenizer
        int parenLevel = -1;            // Current level of parenthesization
        int functionLevel = -1;         // Current level of function nesting
        bool hasDecimal;                // Current number TokenText contains a decimal
        Token ? addToken = null;        /* Stores a TokenText that was initialized somewhere other than the end of the Tokenize() loop
                                         * (this is used for creating Tokens of a different subclass that hold extra information) */
        
        // Information for the current TokenText
        char current = '\0';            // Current character of the input string
        int currentIndex = -1;          // Curent index in the input string
        TokenCategory category = TokenCategory.Uncategorized;           // Category of the current TokenText
        string TokenText = "";              // String content of the current TokenText
        int TokenTextStart = 0;             // Index that the current TokenText started at

        public Tokenizer(string input)
        {
            this.input = input;
            Tokens = new List<Token>();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Tokens.Count; i++)
            {
                if (i == currentIndex)
                {
                    sb.Append(" <" + Tokens[i].ToString() + "> ");
                }
                else
                {
                    sb.Append(Tokens[i] + " ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Advances to the next character in the input string
        /// </summary>
        /// <returns></returns>
        private bool NextChar()
        {
            if (currentIndex >= input.Length - 1) return false;

            currentIndex++;
            current = input[currentIndex];
            return true;
        }

        /// <summary>
        /// If a TokenText has already been created and stored in addToken, add that to Tokens.
        /// Otherwise, add thew newToken that is passed in.
        /// </summary>
        /// <param name="newToken"></param>
        private void AddToken()
        {
            ImplicitMultiplication();   // Before adding the TokenText, check if there is an implicit multiplication

            if (addToken != null)
            {
                Tokens.Add(addToken);
                addToken = null;
            }
            else
            {
                Tokens.Add(new Token(TokenText, category, TokenTextStart, currentIndex + 1));
            }

            // Reset the category and TokenText to indicate that we are starting a new TokenText
            category = TokenCategory.Uncategorized; 
            TokenText = "";
        }

        /// <summary>
        /// Tokenizes all characters of the input string. This is called by the constructor.
        /// </summary>
        public List<Token> Tokenize()
        {
            while(NextChar())
            {
                if (current.Equals(' ')) {
                    /*  Whitespace will be skipped and end any TokenText except for Numbers. In Numbers, the user may wish to use whitespace
                     *  to separate digits similar to commas. */
                    if (category == TokenCategory.Uncategorized || category == TokenCategory.Number)
                    {
                        continue;
                    }
                    else AddToken();
                }

                switch (category)
                {
                    case TokenCategory.Uncategorized:
                        hasDecimal = false;
                        // Each of these TokenText methods returns a boolean indicating if the TokenText was completed
                        if (StartToken()) break;
                        else continue;
                    case TokenCategory.Variable:
                        if (TokenizeVariable()) break;
                        else continue;
                    case TokenCategory.Number:
                        if (TokenizeNumber()) break;
                        else continue;
                    case TokenCategory.Command:
                        if (TokenizeCommand()) break;
                        else continue;
                }
                AddToken();
            }

            if (category != TokenCategory.Uncategorized)
            {   // If a category is set, we reached the end of the input while still reading a single TokenText. This final TokenText still needs to be added.
                AddToken();
            }

            return Tokens;
        }


        /// <summary>
        /// Handles starting a new TokenText. Categorizes the TokenText based on the first letter.
        /// </summary>
        /// <returns>bool indicating whether the current TokenText is completed or needs to continue to be read</returns> 
        private bool StartToken()
        {
            TokenText += current;
            TokenTextStart = currentIndex;

            /* If the TokenText starts with a letter, then it is a Variable or Function. It will be marked as a variable
             * and stay as such unless a [ is found indicating it is a function */
            if (char.IsLetter(current))
            {
                category = TokenCategory.Variable;
                return false;
            }

            // If the TokenText starts with a digit or a decimal, then it is a Number of an unknown amount of digits
            else if (char.IsDigit(current) || current == '.')
            {
                category = TokenCategory.Number; // n indicates Number
                if(current == '.')
                {
                    TokenText = "0."; // If the first character of the number is a decimal, change it to "0." to avoid parsing errors
                    hasDecimal = true;
                }
                return false;
            }

            else if (operators.Contains(current))
            {
                return TokenizeOperator();
            }
            else if (current == '=')
            {
                category = TokenCategory.Assignment;
            }
            else if (current == '(' || current == ')')
            {
                TokenizeParenthesis();
            }
            else if (current == '[' || current == ']')
            {
                TokenizeBracket();
            }
            else if (current == ',' && functionLevel > -1)   // ',' only makes a TokenText if we are parsing the arguments of a function
            {
                addToken = new LevelToken(",", TokenCategory.Comma, currentIndex, currentIndex + 1, functionLevel);
            }
            else if (current == '?')
            {
                category = TokenCategory.Inquiry;
            }
            else if (current == '>')
            {
                category = TokenCategory.Command;
                TokenText = "";     // We don't want to include the '>' in the TokenText
                return false;
            }
            else
            {
                // Any other character is invalid for the start of a TokenText
                ExceptionController.AddException("Invalid character '" + current + "' for start of TokenText.", currentIndex, currentIndex + 1, 'T');
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a number TokenText
        /// </summary>
        /// <returns>bool indicating whether the current TokenText is completed or needs to continue to be read</returns>
        private bool TokenizeNumber()
        {
            // This character continues the TokenText only if it is a digit or decimal
            if (char.IsDigit(current))
            {
                TokenText += current;
                return false;
            }
            else if (current == ',' && functionLevel == -1)
            {   // Commas can be in numbers and are simply ignored UNLESS the TokenTextizer is inside of a functions brackets, because then they separate arguments
                return false; 
            }
            else if (current == '.')
            {
                if (hasDecimal)
                {
                    ExceptionController.AddException("Number contains multiple decimal points.", currentIndex, currentIndex + 1, 'T');
                }
                else
                {
                    hasDecimal = true;
                    TokenText += current;
                    return false;
                }
            }
            else
            {
                /*  If none of the above, then the current TokenText ends and we will start a new one.
                *  We must move the counter (i) back so the loop will check the current character again */
                currentIndex--;
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a variable TokenText
        /// </summary>
        /// <returns>bool indicating whether the current TokenText is completed or needs to continue to be read</returns>
        private bool TokenizeVariable()
        {
            // This character continues the TokenText if it is a letter or digit
            if (char.IsLetter(current) || char.IsDigit(current))
            {
                TokenText += current;
                return false;
            }

            if (current == '[')
            {
                // If we encounter a [, this indicates this variable is actually a function
                addToken = new FunctionToken(TokenText, TokenCategory.Function, TokenTextStart, currentIndex, ++functionLevel);
            }

            /*  The current TokenText ends and we will start a new one.
             *  We must move the counter back so the loop will check the current character again */
            currentIndex--;
            return true;
        }

        /// <summary>
        /// Tokenizes an operator TokenText
        /// </summary>
        /// <returns>bool indicating whether the current TokenText is completed or needs to continue to be read</returns>
        private bool TokenizeOperator()
        {
            category = TokenCategory.Operator;

            /* Before checking the other operators, we first must handle a special case for '-' 
             * to check if it should be the unary negation operator instead of subtraction.     */
            if (current == '-') {
                TokenCategory prevCategory = Tokens.Count > 0 ? Tokens[Tokens.Count - 1].category : TokenCategory.Uncategorized;
                switch (prevCategory)
                {
                    case TokenCategory.Uncategorized:
                    case TokenCategory.Operator:
                    case TokenCategory.OpenParen:
                    case TokenCategory.OpenBracket:
                    case TokenCategory.Comma:
                        // Unary Negation is TokenTextized with the special category '-' (instead of the typical operator 'o')
                        category = TokenCategory.Negation;
                        return true;
                }
 
            }

            // The only operator that is more than one character is // for integer division. Special case.
            if (current == '/')
            {
                if (currentIndex + 1 < input.Length && input[currentIndex + 1] == '/')
                {
                    TokenText = "//";
                    NextChar(); // Move past the second '/' since we have already TokenTextized it
                }
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a parenthesis TokenText
        /// </summary>
        private void TokenizeParenthesis()
        {

            if (current == '(')
            {
                category = TokenCategory.OpenParen;
                addToken = new LevelToken(TokenText, category, TokenTextStart, currentIndex + 1, ++parenLevel);
            }
            else
            {
                category = TokenCategory.CloseParen;
                addToken = new LevelToken(TokenText, category, TokenTextStart, currentIndex + 1, parenLevel--);
            }
        }

        private void TokenizeBracket()
        {

            if (current == '[')
            {
                category = TokenCategory.OpenBracket;
                if (Tokens.Count == 0 || Tokens[Tokens.Count - 1].category != TokenCategory.Function)
                {   // Brackets can only occur after a function TokenText
                    ExceptionController.AddException("TokenCategory.OpenBracket must immediately follow a function name.", currentIndex, currentIndex + 1, 'T');
                    functionLevel = 0; // Avoid letting parenLevel be negative when the TokenText is added, which would cause errores elsewhere
                }
                addToken = new LevelToken(TokenText, category, TokenTextStart, currentIndex + 1, functionLevel);
                // Don't increment functionLevel here because the intiial function TokenText 
            }
            else
            {
                category = TokenCategory.CloseBracket;
                if (functionLevel == -1)
                {
                    ExceptionController.AddException("Unmatched closing bracket.", currentIndex, currentIndex + 1, 'T');
                    functionLevel = 0;  /* To avoid a negative functionLevel, which could cause errors elsewhere
                                         *  (namely when we attempt to color the TokenText, which doesn't matter since it
                                         *  will be red due to the exception anyway) */
                }
                addToken = new LevelToken(TokenText, category, TokenTextStart, currentIndex + 1, functionLevel--);
            }
        }

        private bool TokenizeCommand()
        {
            if (char.IsLetter(current))
            {
                TokenText += current;
                return false;
            }
            currentIndex--;
            return true;
        }

        private void ImplicitMultiplication()
        { 
            if(Tokens.Count >= 1)
            {   // Implicit multiplication can only occur if there was at least one TokenText prior to the current one
                
                switch (category)
                {
                    case TokenCategory.Number:
                    case TokenCategory.Variable:
                    case TokenCategory.Function:
                    case TokenCategory.OpenParen:
                        TokenCategory prevCategory = Tokens[Tokens.Count - 1].category;
                        switch (prevCategory)
                        {
                            case TokenCategory.Number:
                            case TokenCategory.Variable:
                            case TokenCategory.CloseBracket:
                            case TokenCategory.CloseParen:
                                /* Add a new * TokenText of length 0. We do this directly to the list instead of using AddToken because the current TokenText may have
                                 * populated addToken, which then would add that TokenText instead of the multiplication. */
                                Tokens.Add(new Token("*", TokenCategory.Operator, currentIndex, currentIndex));
                                break;
                        }
                        break;
                }
            }
        }
    }
}
