using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Tokenizer
    {
        private string input;                       // The raw string input
        private List<Token> tokens;                 // Stores the tokens that get created
        string assignVariable = "";
        CustomFunction defineFunction = null;

        private static char[] operators = { '+', '-', '*', '/', '^', '%', '!', '=' };

        // Flags and variables that store temporary information for the Tokenizer
        bool hasAssignment = false;     // Assignment Operator encountered
        int parenLevel = -1;             // Current level of parenthesization
        int functionLevel = -1;          // Current level of function nesting
        bool hasDecimal;                // Current number token contains a decimal
        Token addToken;                 /* Stores a token that was initialized somewhere other than the end of the Tokenize() loop
                                         * (this is used for creating tokens of a different subclass that hold extra information) */
        
        // Information for the current token
        char current = '\0';            // Current character of the input string
        int currentIndex = -1;          // Curent index in the input string
        char category = '\0';           // Category of the current token
        string token = "";              // String content of the current token
        int tokenStart = 0;             // Index that the current token started at

        public Tokenizer(string input)
        {
            this.input = input;
            tokens = new List<Token>(input.Length / 2);
            // Starts tokens with a capacity of 50% of input.Length to avoid the initial resizings which would almost always occur

            if (input.Length > 0) Tokenize();
        }


        public List<Token> GetTokens()
        {
            return tokens;
        }

        public string GetAssignVariable()
        {
            return assignVariable;
        }

        public CustomFunction GetDefineFunction()
        {
            return defineFunction;
        }


        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Count(); i++)
            {
                if (i == currentIndex)
                {
                    sb.Append(" <" + tokens[i].ToString() + "> ");
                }
                else
                {
                    sb.Append(tokens[i] + " ");
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
        /// If a token has already been created and stored in addToken, add that to tokens.
        /// Otherwise, add thew newToken that is passed in.
        /// </summary>
        /// <param name="newToken"></param>
        private void AddToken(Token newToken)
        {
            if(addToken != null)
            {
                tokens.Add(addToken);
                addToken = null;
            }
            else
            {
                tokens.Add(newToken);
            }
        }

        /// <summary>
        /// Tokenizes all characters of the input string. This is called by the constructor.
        /// </summary>
        private void Tokenize()
        {
            while(NextChar())
            {
                if (current.Equals(' ')) { // Whitespace is ignored entirely-- even for symbol names and numbers
                    continue;
                }

                switch (category)
                {
                    case '\0': // If a category hasn't yet been determined, we need to start with 
                        hasDecimal = false;
                        if (StartToken()) break;
                        else continue;
                    case 'v':
                        if (TokenizeVariable()) break;
                        else continue;
                    case 'n':
                        if (TokenizeNumber()) break;
                        else continue;
                }
                
                AddToken(new Token(token, category, tokenStart, currentIndex + 1));

                category = '\0'; // Change the category to indicate that we are starting a new token
                token = "";
            }

            if (category == 'v' || category == 'n' || category == 'f') // If the last token is a symbol, number, or function, it hasn't been added yet
            {
                AddToken(new Token(token, category, tokenStart, input.Length));
            }

            // Check if all parentheses have been matched
            if(parenLevel != -1)
            {
                CheckParentheses();
            }

            /* If an assignment operator = has been used, we will save the variable that is being assigned and send it to the evaluator.
             * Then we will remove this variable and the assignment operator from the tokens so the rest of the expression can be parsed normally.
             * We only do this if the Tokenizer hasn't encountered any errors.
             */
            if (hasAssignment && ExceptionController.Count() == 0)
            {
                PrepareAssignment();
            }
        }


        /// <summary>
        /// Handles starting a new token. Categorizes the token based on the first letter.
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns> 
        private bool StartToken()
        {
            token += current;
            tokenStart = currentIndex;
            bool tokenFinished;

            /* If the token starts with a letter, then it is a Variable or Function. It will be marked as a variable
             * and stay as such unless a [ is found indicating it is a function */
            if (Char.IsLetter(current))
            {
                category = 'v'; // v indicates Variable
                tokenFinished = false;
            }

            // If the token starts with a digit or a decimal, then it is a Number of an unknown amount of digits
            else if (Char.IsDigit(current) || current == '.')
            {
                category = 'n'; // n indicates Number
                if(current == '.')
                {
                    token = "0."; // If the first character of the number is a decimal, change it to "0." to avoid parsing errors
                    hasDecimal = true;
                }
                tokenFinished = false;
            }

            else if (operators.Contains(current))
            {
                category = 'o';
                tokenFinished = TokenizeOperator();
            }
            else if (current == '(' || current == ')')
            {
                category = current;

                TokenizeParenthesis();
                tokenFinished = true;
            }
            else if (current == '[' || current == ']')
            {
                category = current;

                TokenizeBracket();
                tokenFinished = true;
            }
            else if (current == ',' && functionLevel > -1)   // ',' only makes a token if we are parsing the arguments of a function
            {
                addToken = new LevelToken(",", ',', currentIndex, currentIndex + 1, functionLevel);
                tokenFinished = true;
            }
            else
            {
                // Any other character is invalid for the start of a token
                ExceptionController.AddException("Invalid character '" + current + "' for start of token.", currentIndex, currentIndex + 1, 'T');
                tokenFinished = true;
            }
            ImplicitMultiplication();   // We need to check for implicit multiplication at the end of this function so the current token is categorized
            return tokenFinished;
        }

        /// <summary>
        /// Tokenizes a number token
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns>
        private bool TokenizeNumber()
        {
            // This character continues the token only if it is a digit or decimal
            if (Char.IsDigit(current))
            {
                token += current;
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
                    ExceptionController.AddException("Number contains multiple decimal points.", currentIndex, currentIndex + 1, 'T');
                }
                else
                {
                    hasDecimal = true;
                    token += current;
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
            if (Char.IsLetter(current) || Char.IsDigit(current))
            {
                token += current;
                return false;
            }

            if (current == '[')
            {
                // If we encounter a [, this indicates this variable is actually a function
                addToken = new FunctionToken(token, 'f', tokenStart, currentIndex, ++functionLevel);
            }

            /*  The current token ends and we will start a new one.
                *  We must move the counter (i) back so the loop will check the current character again */
            currentIndex--;
            return true;
        }

        /// <summary>
        /// Tokenizes an operator token
        /// </summary>
        /// <returns>bool indicating whether the current token is completed or needs to continue to be read</returns>
        private bool TokenizeOperator()
        {
            /* Before checking the other operators, we first must handle a special case for '-' 
             * to check if it should be the unary negation operator instead of subtraction.     */
            if (current == '-') {
                char prevCategory = tokens.Count() > 0 ? tokens[tokens.Count - 1].GetCategory() : '\0';
                switch (prevCategory)
                {
                    case '\0':
                    case 'o':
                    case '(':
                    case '[':
                        category = 'n';     // It should be interpreted as a number isntead of an operator
                        token = "-0";       // Change token to avoid it causing errors when it is parsed as a double
                        return false;
                }
 
            }

            if (current == '=')
            {
                if (hasAssignment)
                {
                    ExceptionController.AddException("Expression contains multiple assignment operators '='.", currentIndex, currentIndex + 1, 'T');
                }
                hasAssignment = true;
            }

            // The only operator that is more than one character is // for integer division. Special case.
            if (current == '/')
            {
                if (currentIndex + 1 < input.Length && input[currentIndex + 1] == '/')
                {
                    token = "//";
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
            if (current == '(') addToken = new LevelToken(token, category, tokenStart, currentIndex, ++parenLevel);
            else
            {
                if (parenLevel < 0)
                {
                    ExceptionController.AddException("Unmatched closing parenthesis.", currentIndex, currentIndex + 1, 'T');
                    parenLevel = 0;   // Avoid letting parenLevel be negative when the token is added, which would cause errores elsewhere
                }
                addToken = new LevelToken(token, category, tokenStart, currentIndex, parenLevel--);
            }
        }

        private void TokenizeBracket()
        {

            if (current == '[')
            {
                if (tokens.Count() == 0 || tokens[tokens.Count() - 1].GetCategory() != 'f')
                {   // Brackets can only occur after a function token
                    ExceptionController.AddException("'[' must immediately follow a function name.", currentIndex, currentIndex + 1, 'T');
                    functionLevel = 0; // Avoid letting parenLevel be negative when the token is added, which would cause errores elsewhere
                }
                addToken = new LevelToken(token, category, tokenStart, currentIndex + 1, functionLevel);
                // Don't increment functionLevel here because the intiial function token 
            }
            else
            {
                if (functionLevel == -1)
                {
                    ExceptionController.AddException("Unmatched closing bracket.", currentIndex, currentIndex + 1, 'T');
                    functionLevel = 0;  /* To avoid a negative functionLevel, which could cause errors elsewhere
                                         *  (namely when we attempt to color the token, which doesn't matter since it
                                         *  will be red due to the exception anyway) */
                }
                addToken = new LevelToken(token, category, tokenStart, currentIndex + 1, functionLevel--);
            }
        }

        /// <summary>
        /// Finds which open parentheses tokens are unmatched by a closing parentheses at the same level.
        /// This method is only called after it is known that there is at least one unmatched parenthesis
        /// </summary>
        private void CheckParentheses()
        {
            for (int i = 0; i < tokens.Count(); i++)
            {
                bool matched = false;
                if (tokens[i].GetCategory() == '(')
                {
                    LevelToken openParen = (LevelToken)tokens[i];
                    for (int j = i + 1; j < tokens.Count(); j++)
                    {
                        if (tokens[j].GetCategory() == ')')
                        {
                            LevelToken closeParen = (LevelToken)tokens[j];
                            if (openParen.GetLevel() == closeParen.GetLevel())
                            {
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        ExceptionController.AddException("Unmatched open parenthesis.", openParen.GetStart(), openParen.GetEnd(), 'T');
                    }

                }
            }
        }

        private void ImplicitMultiplication()
        { 
            if(tokens.Count() >= 1)
            {   // Implicit multiplication can only occur if there was at least one token prior to the current one
                
                switch (category)
                {
                    case 'n':
                    case 'v':
                    case 'f':
                    case '(':
                        char prevCategory = tokens[tokens.Count() - 1].GetCategory();
                        if (prevCategory == 'n' || prevCategory == 'v' || prevCategory == ']' || prevCategory == ')')
                        {
                            /* Add a new * token of length 0. We do this directly to the list instead of using AddToken because the current token may have
                             * populated addToken, which then would add that token instead of the multiplication. */
                            tokens.Add(new Token("*", 'o', currentIndex, currentIndex));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// After tokenization is completed, if an assignment operator was encountered this method will be called.
        /// Determines which token is the variable that will store the resulting value. This token is sent to be stored in
        /// evaluator then is removed from the token list.
        /// </summary>
        private void PrepareAssignment()
        {
            int assignmentIdx = 0;
            int variableIdx;
            for (; assignmentIdx < tokens.Count; assignmentIdx++)
            {   // Find the assignment operator. It is known that there is only one because we checked that there aren't any exceptions.
                if (tokens[assignmentIdx].GetToken().Equals("=")){ break; }
            }
            /*  Assignment can only take place at the beginning or end of the tokens list.
             *  At least one operand must be a variable
             */

            if (assignmentIdx == 0 || assignmentIdx == tokens.Count() - 1)
            {   // Assignment operator is the first or last token
                ExceptionController.AddException("Assignment needs two operands.", tokens[assignmentIdx].GetStart(), tokens[assignmentIdx].GetEnd(), 'T');
                return;
            }

            if  (assignmentIdx != 1     &&   assignmentIdx != tokens.Count() - 2)    // Assignment token is not second or second to last           
            {
                // Check if this is an attempt to define a user function
                if (TokenizeCustomFunction(assignmentIdx)) return;

                ExceptionController.AddException("Assignment can only be done to an isolated variable", tokens[assignmentIdx].GetStart(), tokens[assignmentIdx].GetEnd(), 'T');
                return;
            }

            Token prevToken = tokens[assignmentIdx - 1];
            Token nextToken = tokens[assignmentIdx + 1];

            if (prevToken.GetCategory() == 'v' && nextToken.GetCategory() == 'v') // If both are variables
            {
                if (SymbolTable.variables.ContainsKey(nextToken.GetToken()))
                {   // So long as next is defined, we assign prev <= next (leftward asignment by default)
                    variableIdx = assignmentIdx - 1;
                }
                else if (SymbolTable.variables.ContainsKey(prevToken.GetToken()))
                {   // If next isn't defined but prev is, then we do rightward assignment - prev => next
                    variableIdx = assignmentIdx + 1;
                }
                else
                {   // Otherwise, neither are defined and we cannot successfully assign either
                    ExceptionController.AddException("Cannot assign between two undefined variables.", prevToken.GetStart(), nextToken.GetEnd(), 'T');
                    return;
                }

            }
            else
            {
                if (prevToken.GetCategory() == 'v')
                {   // prev is the token that will receive the assignment
                    variableIdx = assignmentIdx - 1;
                }
                else if (nextToken.GetCategory() == 'v')
                {   // next is the token that will receive the assignment
                    variableIdx = assignmentIdx + 1;
                }
                else
                {   // Neither operand is a variable

                    // Check if this is an attempt to define a user function
                    if (TokenizeCustomFunction(assignmentIdx)) return;

                    ExceptionController.AddException("At least one operand must be a variable for assignment", prevToken.GetStart(), nextToken.GetEnd(), 'T');
                    return;
                }
            }

            string variableName = tokens[variableIdx].GetToken();

            // Removing from the List will shift indices. We need to remove the lowest index twice to remove the variable and the assignment operator
            tokens.RemoveAt(Math.Min(assignmentIdx, variableIdx));
            tokens.RemoveAt(Math.Min(assignmentIdx, variableIdx));

            assignVariable = variableName;
        }



        private bool TokenizeCustomFunction(int assignmentIdx)
        {
            List<Token> parameters = new List<Token>();

            if (assignmentIdx < 3 || tokens[0].GetCategory() != 'f' || 
                tokens[1].GetCategory() != '[' || tokens[assignmentIdx - 1].GetCategory() != ']')
            {   // There must be at least three tokens before the assignment: 'f' '[' ']'. Otherwise don't interpret this as a user function
                return false;
            }


            FunctionToken functionToken = (FunctionToken)tokens[0];
            if (    SymbolTable.functions.ContainsKey(functionToken.GetToken()) 
                &&  SymbolTable.functions[functionToken.GetToken()] is PrimitiveFunction)
            {
                ExceptionController.AddException("Cannot redefine primitive function '" + functionToken.GetToken() + "'.", functionToken.GetStart(), assignmentIdx + 1, 'T');
                return false;
            }

            tokens.RemoveAt(0);     // Remove functionToken
            tokens.RemoveAt(0);     // Remove '['

            int parameterCount = 0;
            while (tokens.Count() > 0)
            {
                if (tokens[0].GetCategory() == ']' && functionToken.GetLevel() == ((LevelToken)tokens[0]).GetLevel())
                {   // Break the loop when we find the ']' that terminates this function token
                    break;
                }
                Token token = tokens[0];
                switch (token.GetCategory())
                {
                    case 'v':
                        // If it is a variable, add it to the local variables so when we parse the function definition we see that the parameters are defined
                        parameterCount++;
                        parameters.Add(token);
                        break;
                    case ',':
                        // Skip over the commas
                        break;
                    default:
                        ExceptionController.AddException("Invalid token '" + token + "' in function signature.", token.GetStart(), token.GetEnd(), 'T');
                        return false;
                }
                tokens.RemoveAt(0);
            }
            tokens.RemoveAt(0);     // Remove ']'
            tokens.RemoveAt(0);     // Remove '='

            defineFunction = new CustomFunction(functionToken.GetToken(), parameters);
            return true;
        }
    }
}
