using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            currentIndex = 0;
            this.executeFunctions = executeFunctions;
        }

        public Parser(List<Token> tokens, bool executeFunctions) 
            : this(tokens, executeFunctions, new SymbolTable()) {   }

        /// <summary>
        /// Checks that the current token (tokens[i]) is the same as matchWith. Avoids out of bounds errors.
        /// </summary>
        /// <param name="matchWith"></param> The string we are comparing against the current token
        /// <returns></returns>
        public bool MatchToken(string matchWith)
        {
            if (currentIndex >= tokens.Count) return false;
            return tokens[currentIndex].GetToken().Equals(matchWith);
        }

        public double ParseExpression()
        {
            if (tokens.Count == 0) return 1;
            double left = ParseTerm();
            while (MatchToken("+") || MatchToken("-"))
            {
                string op = tokens[currentIndex].GetToken();
                currentIndex++; // Move past operator
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
                string op = tokens[currentIndex].GetToken();
                currentIndex++; // Move past operator
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
            double left = ParsePrimary(
                );
            while (MatchToken("^") || MatchToken("!"))
            {
                string op = tokens[currentIndex].GetToken();
                currentIndex++; // Move past operator
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
                ExceptionController.AddException("Input ends with operator '" + prevToken.GetToken() + "', which does not have enough operands.", prevToken.GetStart(), prevToken.GetEnd(), 'P');
                return 1;
            }
            Token token = tokens[currentIndex];
            double value = 0;
            switch (token.GetCategory())
            {
                case TokenCategory.Number:
                    value = double.Parse(tokens[currentIndex].GetToken());
                    break;
                case TokenCategory.OpenParen:
                    currentIndex++;    // Skip TokenCategory.OpenParen token
                    value = ParseExpression();
                    break;
                case TokenCategory.Variable:
                case TokenCategory.Parameter:
                    if (localVariables.LocalsContains(token.GetToken()))
                    {   // Check if this variable is defined locally, since local variables should overshadow globals
                        value = localVariables.GetLocal(token.GetToken());
                    }
                    else if (SymbolTable.variables.ContainsKey(token.GetToken()))
                    {
                        value = SymbolTable.variables[token.GetToken()].GetValue();
                    }
                    else
                    {
                        ExceptionController.AddException("Undefined variable '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    }
                    break;
                case TokenCategory.Function:
                    value = ParseFunction();
                    break;
                case TokenCategory.Negation:
                    // Unary negation
                    currentIndex++;     // Skip '-' token
                    value = -1 * ParseExpression();
                    break;
                case TokenCategory.Inquiry:
                    InquireFunction();
                    break;
                default:
                    // If the switch falls to the default, there was a token we didn't account for.
                    ExceptionController.AddException("Encountered an unexpected token '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    break;
            }
            currentIndex++;
            return value;
        }

        /// <summary>
        /// Parses a function token and arguments. Executes the function if needed.
        /// </summary>
        /// <returns></returns> If the function should be executed, returns the result of the function call. If not, returns 1 as a dummy value.
        private double ParseFunction(bool inquireFunction = false)
        {
            FunctionToken functionToken = (FunctionToken)tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(functionToken.GetToken()))
            {   // functionToken is not in the functions hash table
                ExceptionController.AddException("Undefined function '" + functionToken.GetToken() + "'.", functionToken.GetStart(), functionToken.GetEnd(), 'P');
                return 0;
            }

            CalculatorFunction function = SymbolTable.functions[functionToken.GetToken()];

            List<double> arguments = ParseArguments();

            if (arguments.Count != function.GetNumParameters())
            {
                ExceptionController.AddException("Function '" + functionToken + "' requires " +
                                        function.GetNumParameters() + " arguments, received " + arguments.Count + ".",
                                        functionToken.GetStart(), functionToken.GetEnd(), 'P');
            }

            if (executeFunctions && ExceptionController.GetCount() == 0)
            {   // Only execute the function if this Evaluator is set to execute functions and there have been no exceptions
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
                if (tokens[currentIndex].GetCategory() == TokenCategory.CloseBracket && ((LevelToken)tokens[currentIndex]).GetLevel() == functionToken.GetLevel()) break;

                if (tokens[currentIndex].GetCategory() == TokenCategory.Comma && ((LevelToken)tokens[currentIndex]).GetLevel() == functionToken.GetLevel())
                {   // If we find a ',' of the same level as the current function, parse the tokens of the argument and add the resulting double to the functionToken's arguments list

                    Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                    // Parse this argument. Any errors will be added to the same evaluator

                    arguments.Add(parseArg.ParseExpression());
                    argumentTokens = new List<Token>();
                }
                else
                {   // Any token other than ',' comprises the argument, so add it to the argumentsTokens list
                    argumentTokens.Add(tokens[currentIndex]);
                }
                currentIndex++; // Move past TokenCategory.CloseBracket
            }

            if (currentIndex >= tokens.Count)
            {   // If the loop didn't find a TokenCategory.CloseBracket, there is an unmatched open bracket
                ExceptionController.AddException("Unmatched open bracket.", currentIndex - 1, currentIndex - 1, 'P');
            }

            if (argumentTokens.Count > 0)
            {   // Add the last argument if it has at least one token
                Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                arguments.Add(parseArg.ParseExpression());
            }

            return arguments;
        }

        /// <summary>
        /// Replaces an inquired function token with its source tokens, with any parameters replaced with the associated
        /// parsed argument values.
        /// </summary>
        private void InquireFunction()
        {
            tokens.RemoveAt(currentIndex);  // Remove '?' token
            Token token = tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(token.GetToken()))
            {
                ExceptionController.AddException("Cannot inquire on undefined function '" + token + "'.",
                                                    token.GetStart(), token.GetEnd(), 'P');
                return;
            }
            if(SymbolTable.functions[token.GetToken()] is PrimitiveFunction)
            {
                ExceptionController.AddException("Cannot inquire on primitive function '" + token + "'. Inquiry can only be " +
                                                    "done to custom functions and variables.", token.GetStart(), token.GetEnd(), 'P');
                return;
            }

            CustomFunction customFunction = (CustomFunction)SymbolTable.functions[token.GetToken()];
            
            int functionStartIndex = currentIndex;

            List<double> arguments = ParseArguments();
            // ParseArguments will move currentIndex past the TokenCategory.CloseBracket that ends this function

            if(arguments.Count != 0 && arguments.Count != customFunction.GetNumParameters())
            {
                ExceptionController.AddException("A function call can only be inquired if it has no arguments or the exact number of arguments " +
                    "the function requires. '" +  token + "' requires " + customFunction.GetNumParameters() + " arguments.", token.GetStart(), token.GetEnd(), 'P');
            }

            if (executeFunctions)
            {
                List<Token> inquiryTokens = customFunction.Inquire(arguments);
                tokens.RemoveRange(functionStartIndex, currentIndex - functionStartIndex + 1);  // Remove the function tokens from the function to the TokenCategory.CloseBracket
                tokens.Insert(functionStartIndex, new Token("(", TokenCategory.OpenParen, 0, 0));   // Add TokenCategory.OpenParen token
                tokens.InsertRange(functionStartIndex + 1, inquiryTokens);  // Add the inquiry tokens
                currentIndex = functionStartIndex + inquiryTokens.Count + 1;  // Move currentIndex past the TokenCategory.OpenParen and inquiryTokens
                tokens.Insert(currentIndex, new Token(")", TokenCategory.CloseParen, 0, 0));   // Add TokenCategory.CloseParen token
            }
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
