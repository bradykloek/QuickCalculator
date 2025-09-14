using System.Text;
using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    /// <summary>
    /// Implements a Recursive Descent algorithm to parse expressions
    /// 
    /// expression:     term +- term
    /// term:           factor */ factor
    /// factor:         primary ^ factor
    /// primary:        number, variable, ( expression )
    /// </summary>
    internal class Parser
    {
        private List<Token> tokens;
        private int currentIndex;
        private SymbolTable localVariables;
        private bool executeFunctions;


        /// <summary>
        /// Initializes a Parser using a completed token list. This will parse and evaluate the tokens,
        /// and store the result in this.result.
        /// </summary>
        /// <param name="tokens"></param>
        /// 

        public Parser(List<Token> tokens, bool executeFunctions, SymbolTable localVariables)
        {
            this.localVariables = localVariables;
            this.tokens = tokens;
            this.executeFunctions = executeFunctions;
            currentIndex = -1;
            NextToken();
        }

        public Parser(List<Token> tokens, bool executeFunctions) 
            : this(tokens, executeFunctions, new SymbolTable()) {   }

        /// <summary>
        /// Checks that the current token (tokens[i]) is the same as matchWith. Avoids out of bounds errors.
        /// </summary>
        /// <param name="matchWith"></param> The string we are comparing against the current token
        /// <returns></returns>
        private bool MatchToken(string matchWith)
        {
            if (currentIndex >= tokens.Count) return false;
            return tokens[currentIndex].TokenText.Equals(matchWith);
        }

        private void NextToken()
        {
            currentIndex++;
            while (currentIndex < tokens.Count && tokens[currentIndex].Skip)
            {
                currentIndex++;
            }
        }

        public double ParseExpression()
        {
            if (tokens.Count == 0) return 1;
            double left = ParseTerm();
            while (MatchToken("+") || MatchToken("-"))
            {
                string op = tokens[currentIndex].TokenText;
                NextToken(); // Move past operator
                double right = ParseTerm();
                left = op.Equals("+") ? left + right : left - right;
            }
            return left;
        }

        private double ParseTerm()
        {
            double left = ParseFactor();
            while (MatchToken("*") || MatchToken("/") || MatchToken("%") || MatchToken("//"))
            {
                string op = tokens[currentIndex].TokenText;
                NextToken(); // Move past operator
                double right = ParseFactor();
                switch (op)
                {
                    case "*":
                        left = left * right;
                        break;
                    case "/":
                        left = left / right;
                        break;
                    case "%":
                        left = left % right;
                        break;
                    case "//":
                        left = Math.Floor(left / right);
                        break;
                }
            }
            return left;
        }

        private double ParseFactor()
        {
            double left = ParsePrimary();
            while (MatchToken("^") || MatchToken("!"))
            {
                string op = tokens[currentIndex].TokenText;
                NextToken(); // Move past operator
                switch (op)
                {
                    case "^":
                        double right = ParseFactor();
                        left = Math.Pow(left, right);
                        break;
                    case "!":
                        // Pass left in as the sole argument for the "factorial" function
                        left = ((PrimitiveFunction)SymbolTable.functions["factorial"]).Execute(new List<double> { left });
                        break;
                }
            }
            return left;
        }

        private double ParsePrimary()
        {
            if (currentIndex >= tokens.Count)
            {
                // If we have gone out of bounds of the Token List, there was an operator at the end of the string which thus didn't have enough operands
                Token prevToken = tokens[currentIndex - 1];
                ErrorController.AddError("Input ends with operator '" + prevToken.TokenText + "', which does not have enough operands.", prevToken.StartIndex, prevToken.EndIndex, ErrorSource.Parser);
                return 1;
            }
            Token token = tokens[currentIndex];
            double value = 0;
            switch (token.Category)
            {
                case TokenCategory.Number:
                    value = double.Parse(tokens[currentIndex].TokenText);
                    NextToken();
                    break;
                case TokenCategory.OpenParen:
                    NextToken();     // Skip ( token
                    value = ParseExpression();
                    NextToken();     // Skip ) token
                    break;
                case TokenCategory.Variable:
                case TokenCategory.Parameter:
                    if (localVariables.LocalsContains(token.TokenText))
                    {   // Check if this variable is defined locally, since local variables should overshadow globals
                        value = Convert.ToDouble(localVariables.GetLocal(token.TokenText));
                    }
                    else if (SymbolTable.variables.ContainsKey(token.TokenText))
                    {
                        value = SymbolTable.variables[token.TokenText].value;
                    }
                    else
                    {
                        ErrorController.AddError("Undefined variable '" + token.TokenText + "'.", token.StartIndex, token.EndIndex, ErrorSource.Parser);
                    }
                    NextToken();
                    break;
                case TokenCategory.Function:
                    value = ParseFunction();
                    break;
                case TokenCategory.Negation:
                    // Unary negation
                    NextToken();     // Skip '-' token
                    value = -1 * ParsePrimary();
                    break;
                default:
                    // If the switch falls to the default, there was a token we didn't account for.
                    ErrorController.AddError("Encountered an unexpected token '" + token.TokenText + "'.", token.StartIndex, token.EndIndex, ErrorSource.Parser);
                    break;
            }
            return value;
        }

        /// <summary>
        /// Parses a function token and arguments. Executes the function if needed.
        /// </summary>
        /// <returns></returns> If the function should be executed, returns the result of the function call. If not, returns 1 as a dummy value.
        private double ParseFunction()
        {
            FunctionToken functionToken = (FunctionToken)tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(functionToken.TokenText))
            {   // functionToken is not in the functions hash table
                ErrorController.AddError("Undefined function '" + functionToken.TokenText + "'.", functionToken.StartIndex, functionToken.EndIndex, ErrorSource.Parser);
                return 0;
            }

            CalcFunction function = SymbolTable.functions[functionToken.TokenText];

            List<double> arguments = ParseArguments();

            if (arguments.Count != function.NumParameters())
            {
                ErrorController.AddError("Function '" + functionToken + "' requires " +
                                        function.NumParameters() + " arguments, received " + arguments.Count + ".",
                                        functionToken.StartIndex, functionToken.EndIndex, ErrorSource.Parser);
            }

            if (executeFunctions && ErrorController.Count() == 0)
            {   // Only execute the function if this Evaluator is set to execute functions and there have been no Errors
                return function.Execute(arguments);
            }
            else
            {   // Otherwise just return a dummy value of 1. The user won't see the result in this case anyway.
                return 1;
            }

            
        }

        /// <summary>
        /// Parses tokens starting at a function token until it reaches the TokenCategory.CloseBracket that terminates the function call.
        /// Each time an individual argument is terminated by a ',' token, it is parsed and added to the list of
        /// doubles that is eventually returned.
        /// </summary>
        /// <param name="functionToken"></param> The token of the function that is being parsed
        /// <returns></returns> A list of doubles representing the parsed arguments
        private List<double> ParseArguments()
        {
            FunctionToken functionToken = (FunctionToken)tokens[currentIndex];
            currentIndex += 2;  // Move past TokenCategory.OpenBracket
            List<double> arguments = new List<double>();
            List<Token> argumentTokens = new List<Token>();
            while (currentIndex < tokens.Count)
            {   // End loop if we find a closing bracket of the same level as the function we are parsing
                if (tokens[currentIndex].Category == TokenCategory.CloseBracket && ((LevelToken)tokens[currentIndex]).Level == functionToken.Level)
                    break;

                if (tokens[currentIndex].Category == TokenCategory.Comma && ((LevelToken)tokens[currentIndex]).Level == functionToken.Level)
                {   // If we find a ',' of the same level as the current function, parse the tokens of the argument and add the resulting double to the functionToken's arguments list

                    Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                    // Parse this argument. Any errors will be added to the same evaluator

                    arguments.Add(parseArg.ParseExpression());
                    argumentTokens.Clear();
                }
                else
                {   // Any token other than ',' comprises the argument, so add it to the argumentsTokens list
                    argumentTokens.Add(tokens[currentIndex]);
                }
                NextToken();
            }

            if (currentIndex >= tokens.Count)
            {   // If the loop didn't find a TokenCategory.CloseBracket, there is an unmatched open bracket
                ErrorController.AddError("Unmatched open bracket.", currentIndex - 1, currentIndex - 1, ErrorSource.Parser);
            }

            if (argumentTokens.Count > 0)
            {   // Add the last argument if it has at least one token
                Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                arguments.Add(parseArg.ParseExpression());
            }

            return arguments;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < tokens.Count; x++)
            {
                if (x == currentIndex)
                {
                    sb.Append(" <" + tokens[x].ToString() + "> ");
                }
                else
                {
                    sb.Append(tokens[x] + " ");
                }
            }
            return sb.ToString();
        }
    }
}
