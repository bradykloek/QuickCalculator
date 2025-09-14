using System.Text;
using QuickCalculator.Errors;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Tokenizer
    {
        private string input;                       // The raw string input
        private List<Token> tokens;                 // Stores the tokens that get created

        public bool IncludesInquiry { get; private set; } = false;  // This stores whether an inquiry (using the '?' operator) is in the input.

        private static HashSet<char> operators = new HashSet<char>{ '+', '-', '*', '/', '^', '%', '!'};

        // Flags and variables that store temporary information for the Tokenizer
        int parenLevel = -1;            // Current level of parenthesization
        int functionLevel = -1;         // Current level of function nesting
        bool hasDecimal;                // Current number token contains a decimal
        Token ? addToken = null;        /* Stores a token that was initialized somewhere other than the end of the Tokenize() loop
                                         * (this is used for creating tokens of a different subclass that hold extra information) */
        
        // Information for the current token
        char current = '\0';            // Current character of the input string
        int currentIndex = -1;          // Current index in the input string
        TokenCategory category = TokenCategory.Uncategorized;           // Category of the current token
        StringBuilder token = new StringBuilder();              // String content of the current token
        int tokenStart = 0;             // Index that the current token started at

        public Tokenizer(string input)
        {
            this.input = input;
            tokens = new List<Token>();
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
        /// If a token has already been created and stored in addToken, add that to tokens.
        /// Otherwise, add the newToken that is passed in.
        /// </summary>
        /// <param name="newToken"></param>
        private void AddToken()
        {
            ImplicitMultiplication();   // Before adding the token, check if there is an implicit multiplication

            if (addToken != null)
            {
                tokens.Add(addToken);
                addToken = null;
            }
            else
            {
                tokens.Add(new Token(token.ToString(), category, tokenStart, currentIndex + 1));
            }

            // Reset the category and token to indicate that we are starting a new token
            category = TokenCategory.Uncategorized; 
            token.Clear();
        }

        /// <summary>
        /// Tokenizes all characters of the input string. This is called by the constructor.
        /// </summary>
        public List<Token> Tokenize()
        {
            while(NextChar())
            {
                if (current.Equals(' ') || current.Equals('\n')) {
                    /*  Whitespace will be skipped and end any token except for Numbers. In Numbers, the user may wish to use whitespace
                     *  to separate digits similar to commas. */
                    if (category != TokenCategory.Uncategorized && category != TokenCategory.Number)
                    {
                        AddToken();
                    }
                    continue;
                }

                switch (category)
                {
                    case TokenCategory.Uncategorized:
                        hasDecimal = false;
                        // Each of these token methods returns a boolean indicating if the token was completed
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
            {   // If a category is set, we reached the end of the input while still reading a single token. This final token still needs to be added.
                AddToken();
            }

            return tokens;
        }


        /// <summary>
        /// Handles starting a new token. Categorizes the token based on the first letter.
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns> 
        private bool StartToken()
        {
            token.Append(current);
            tokenStart = currentIndex;

            /* If the token starts with a letter, then it is a Variable or Function. It will be marked as a variable
             * and stay as such unless a [ is found indicating it is a function */
            if (char.IsLetter(current))
            {
                category = TokenCategory.Variable;
                return false;
            }

            // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
            else if (char.IsDigit(current) || current == '.')
            {
                category = TokenCategory.Number;
                if(current == '.')
                {
                    token.Insert(0,'0'); // If the first character of the number is a decimal, make it to "0." to avoid parsing errors
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
            else if (current == ',' && functionLevel > -1)   // ',' only makes a token if we are parsing the arguments of a function
            {
                addToken = new LevelToken(",", TokenCategory.Comma, currentIndex, currentIndex + 1, functionLevel);
            }
            else if (current == '?')
            {
                category = TokenCategory.Inquiry;
                IncludesInquiry = true;
            }
            else if (current == '>')
            {
                category = TokenCategory.Command;
                token.Clear();     // We don't want to include the '>' in the token
                return false;
            }
            else
            {
                // Any other character is invalid for the start of a token
                ErrorController.AddError("Invalid character '" + current + "' for start of token.", currentIndex, currentIndex + 1, ErrorSource.Tokenizer);
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a number token
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns>
        private bool TokenizeNumber()
        {
            // This character continues the token only if it is a digit or decimal
            if (char.IsDigit(current))
            {
                token.Append(current);
                return false;
            }
            else if (current == ',' && functionLevel == -1)
            {   // Commas can be in numbers and are simply ignored UNLESS the tokenizer is inside of a functions brackets, because then they separate arguments
                return false; 
            }
            else if (current == '.')
            {
                if (hasDecimal)
                {
                    ErrorController.AddError("Number contains multiple decimal points.", currentIndex, currentIndex + 1, ErrorSource.Tokenizer);
                }
                else
                {
                    hasDecimal = true;
                    token.Append(current);
                    return false;
                }
            }
            else
            {
                /*  If none of the above, then the current token ends and we will start a new one.
                *  We must move the counter (i) back so the loop will check the current character again */
                currentIndex--;
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a variable token
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns>
        private bool TokenizeVariable()
        {
            // This character continues the token if it is a letter or digit
            if (char.IsLetter(current) || char.IsDigit(current))
            {
                token.Append(current);
                return false;
            }

            /*  The current token ends and we will start a new one.
             *  We must move the counter back so the loop will check the current character again */
            currentIndex--;
            return true;
        }

        /// <summary>
        /// Tokenizes an operator token
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns>
        private bool TokenizeOperator()
        {
            category = TokenCategory.Operator;

            /* Before checking the other operators, we first must handle a special case for '-' 
             * to check if it should be the unary negation operator instead of subtraction.     */
            if (current == '-') {
                TokenCategory prevCategory = tokens.Count > 0 ? tokens[tokens.Count - 1].Category : TokenCategory.Uncategorized;
                switch (prevCategory)
                {
                    case TokenCategory.Uncategorized:
                    case TokenCategory.Operator:
                    case TokenCategory.OpenParen:
                    case TokenCategory.OpenBracket:
                    case TokenCategory.Comma:
                        category = TokenCategory.Negation;
                        return true;
                }
 
            }

            // The only operator that is more than one character is // for integer division. Special case.
            if (current == '/')
            {
                if (currentIndex + 1 < input.Length && input[currentIndex + 1] == '/')
                {
                    token.Append('/');  // Make the token "//"
                    NextChar(); // Move past the second '/' since we have already tokenized it
                }
            }
            return true;
        }

        /// <summary>
        /// Tokenizes a parenthesis token
        /// </summary>
        private void TokenizeParenthesis()
        {

            if (current == '(')
            {
                category = TokenCategory.OpenParen;
                addToken = new LevelToken(token.ToString(), category, tokenStart, currentIndex + 1, ++parenLevel);
            }
            else
            {
                category = TokenCategory.CloseParen;
                addToken = new LevelToken(token.ToString(), category, tokenStart, currentIndex + 1, parenLevel--);
            }
        }

        private void TokenizeBracket()
        {

            if (current == '[')
            {
                category = TokenCategory.OpenBracket;

                if (tokens.Count == 0 || tokens[tokens.Count - 1].Category != TokenCategory.Variable)
                {   // Brackets can only occur after a function token
                    ErrorController.AddError("TokenCategory.OpenBracket must immediately follow a function name.", currentIndex, currentIndex + 1, ErrorSource.Tokenizer);
                    functionLevel = 0; // Avoid letting functionLevel be negative when the token is added, which would cause errors elsewhere
                }

                // If the previous token was a variable, it should actually be tokenized as a function
                Token varToken = tokens[tokens.Count - 1];
                tokens[tokens.Count - 1] = new FunctionToken(varToken.TokenText, TokenCategory.Function, varToken.StartIndex, varToken.EndIndex, ++functionLevel);
                addToken = new LevelToken(token.ToString(), category, tokenStart, currentIndex + 1, functionLevel);
            }
            else
            {
                category = TokenCategory.CloseBracket;
                if (functionLevel == -1)
                {
                    ErrorController.AddError("Unmatched closing bracket.", currentIndex, currentIndex + 1, ErrorSource.Tokenizer);
                    functionLevel = 0;  /* To avoid a negative functionLevel, which could cause errors elsewhere
                                         *  (namely when we attempt to color the token, which doesn't matter since it
                                         *  will be red due to the exception anyway) */
                }
                addToken = new LevelToken(token.ToString(), category, tokenStart, currentIndex + 1, functionLevel--);
            }
        }

        private bool TokenizeCommand()
        {
            if (char.IsLetter(current))
            {
                token.Append(current);
                return false;
            }
            currentIndex--;
            return true;
        }

        private void ImplicitMultiplication()
        { 
            if(tokens.Count >= 1)
            {   // Implicit multiplication can only occur if there was at least one token prior to the current one
                
                switch (category)
                {
                    case TokenCategory.Number:
                    case TokenCategory.Variable:
                    case TokenCategory.Function:
                    case TokenCategory.OpenParen:
                        TokenCategory prevCategory = tokens[tokens.Count - 1].Category;
                        switch (prevCategory)
                        {
                            case TokenCategory.Number:
                            case TokenCategory.Variable:
                            case TokenCategory.CloseBracket:
                            case TokenCategory.CloseParen:
                                /* Add a new * token of length 0. We do this directly to the list instead of using AddToken because the current token may have
                                 * populated addToken, which then would add that token instead of the multiplication. */
                                tokens.Add(new Token("*", TokenCategory.Operator, currentIndex, currentIndex));
                                break;
                        }
                        break;
                }
            }
        }
    }
}
