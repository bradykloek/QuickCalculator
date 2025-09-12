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
        private List<Token> Tokens;
        private int currentIndex;
        private SymbolTable localVariables;
        private bool executeFunctions;


        /// <summary>
        /// Initializes a Parser using a completed TokenText list. This will parse and evaluate the Tokens,
        /// and store the result in this.result.
        /// </summary>
        /// <param name="Tokens"></param>
        /// 

        public Parser(List<Token> Tokens, bool executeFunctions, SymbolTable localVariables)
        {
            this.localVariables = localVariables;
            this.Tokens = Tokens;
            currentIndex = 0;
            this.executeFunctions = executeFunctions;
        }

        public Parser(List<Token> Tokens, bool executeFunctions) 
            : this(Tokens, executeFunctions, new SymbolTable()) {   }

        /// <summary>
        /// Checks that the current TokenText (Tokens[i]) is the same as matchWith. Avoids out of bounds errors.
        /// </summary>
        /// <param name="matchWith"></param> The string we are comparing against the current TokenText
        /// <returns></returns>
        public bool MatchToken(string matchWith)
        {
            if (currentIndex >= Tokens.Count) return false;
            return Tokens[currentIndex].TokenText.Equals(matchWith);
        }

        public double ParseExpression()
        {
            if (Tokens.Count == 0) return 1;
            double left = ParseTerm();
            while (MatchToken("+") || MatchToken("-"))
            {
                string op = Tokens[currentIndex].TokenText;
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
                string op = Tokens[currentIndex].TokenText;
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
                string op = Tokens[currentIndex].TokenText;
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
            if (currentIndex >= Tokens.Count)
            {
                // If we have gone out of bounds of the Token List, there was an operator at the end of the string which thus didn't have enough operands
                Token prevToken = Tokens[currentIndex - 1];
                ExceptionController.AddException("Input ends with operator '" + prevToken.TokenText + "', which does not have enough operands.", prevToken.StartIndex, prevToken.EndIndex, 'P');
                return 1;
            }
            Token TokenText = Tokens[currentIndex];
            double value = 0;
            switch (TokenText.category)
            {
                case TokenCategory.Number:
                    value = double.Parse(Tokens[currentIndex].TokenText);
                    break;
                case TokenCategory.OpenParen:
                    currentIndex++;    // Skip TokenCategory.OpenParen TokenText
                    value = ParseExpression();
                    break;
                case TokenCategory.Variable:
                case TokenCategory.Parameter:
                    if (localVariables.LocalsContains(TokenText.TokenText))
                    {   // Check if this variable is defined locally, since local variables should overshadow globals
                        value = localVariables.GetLocal(TokenText.TokenText);
                    }
                    else if (SymbolTable.variables.ContainsKey(TokenText.TokenText))
                    {
                        value = SymbolTable.variables[TokenText.TokenText].value;
                    }
                    else
                    {
                        ExceptionController.AddException("Undefined variable '" + TokenText.TokenText + "'.", TokenText.StartIndex, TokenText.EndIndex, 'P');
                    }
                    break;
                case TokenCategory.Function:
                    value = ParseFunction();
                    break;
                case TokenCategory.Negation:
                    // Unary negation
                    currentIndex++;     // Skip '-' TokenText
                    value = -1 * ParseExpression();
                    break;
                case TokenCategory.Inquiry:
                    InquireFunction();
                    break;
                default:
                    // If the switch falls to the default, there was a TokenText we didn't account for.
                    ExceptionController.AddException("Encountered an unexpected TokenText '" + TokenText.TokenText + "'.", TokenText.StartIndex, TokenText.EndIndex, 'P');
                    break;
            }
            currentIndex++;
            return value;
        }

        /// <summary>
        /// Parses a function TokenText and arguments. Executes the function if needed.
        /// </summary>
        /// <returns></returns> If the function should be executed, returns the result of the function call. If not, returns 1 as a dummy value.
        private double ParseFunction(bool inquireFunction = false)
        {
            FunctionToken functionToken = (FunctionToken)Tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(functionToken.TokenText))
            {   // functionToken is not in the functions hash table
                ExceptionController.AddException("Undefined function '" + functionToken.TokenText + "'.", functionToken.StartIndex, functionToken.EndIndex, 'P');
                return 0;
            }

            CalcFunction function = SymbolTable.functions[functionToken.TokenText];

            List<double> arguments = ParseArguments();

            if (arguments.Count != function.NumParameters())
            {
                ExceptionController.AddException("Function '" + functionToken + "' requires " +
                                        function.NumParameters() + " arguments, received " + arguments.Count + ".",
                                        functionToken.StartIndex, functionToken.EndIndex, 'P');
            }

            if (executeFunctions && ExceptionController.Count() == 0)
            {   // Only execute the function if this Evaluator is set to execute functions and there have been no Exceptions
                return function.Execute(arguments);
            }
            else
            {   // Otherwise just return a dummy value of 1. The user won't see the result in this case anyway.
                return 1;
            }

            
        }

        /// <summary>
        /// Parses Tokens starting at a function TokenText until it reaches the TokenCategory.CloseBracket that terminates the function call.
        /// Each time an individual argument is terminated by a ',' TokenText, it is parsed and added to the list of
        /// doubles that is eventually returned.
        /// </summary>
        /// <param name="functionToken"></param> The TokenText of the function that is being parsed
        /// <returns></returns> A list of doubles representing the parsed arguments
        private List<double> ParseArguments()
        {
            FunctionToken functionToken = (FunctionToken)Tokens[currentIndex];
            currentIndex += 2;  // Move past TokenCategory.OpenBracket
            List<double> arguments = new List<double>();
            List<Token> argumentTokens = new List<Token>();
            while (currentIndex < Tokens.Count)
            {   // End loop if we find a closing bracket of the same level as the function we are parsing
                if (Tokens[currentIndex].category == TokenCategory.CloseBracket && ((LevelToken)Tokens[currentIndex]).Level == functionToken.Level) break;

                if (Tokens[currentIndex].category == TokenCategory.Comma && ((LevelToken)Tokens[currentIndex]).Level == functionToken.Level)
                {   // If we find a ',' of the same level as the current function, parse the Tokens of the argument and add the resulting double to the functionToken's arguments list

                    Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                    // Parse this argument. Any errors will be added to the same evaluator

                    arguments.Add(parseArg.ParseExpression());
                    argumentTokens = new List<Token>();
                }
                else
                {   // Any TokenText other than ',' comprises the argument, so add it to the argumentsTokens list
                    argumentTokens.Add(Tokens[currentIndex]);
                }
                currentIndex++; // Move past TokenCategory.CloseBracket
            }

            if (currentIndex >= Tokens.Count)
            {   // If the loop didn't find a TokenCategory.CloseBracket, there is an unmatched open bracket
                ExceptionController.AddException("Unmatched open bracket.", currentIndex - 1, currentIndex - 1, 'P');
            }

            if (argumentTokens.Count > 0)
            {   // Add the last argument if it has at least one TokenText
                Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                arguments.Add(parseArg.ParseExpression());
            }

            return arguments;
        }

        /// <summary>
        /// Replaces an inquired function TokenText with its Source Tokens, with any parameters replaced with the associated
        /// parsed argument values.
        /// </summary>
        private void InquireFunction()
        {
            Tokens.RemoveAt(currentIndex);  // Remove '?' TokenText
            Token TokenText = Tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(TokenText.TokenText))
            {
                ExceptionController.AddException("Cannot inquire on undefined function '" + TokenText + "'.",
                                                    TokenText.StartIndex, TokenText.EndIndex, 'P');
                return;
            }
            if(SymbolTable.functions[TokenText.TokenText] is PrimitiveFunction)
            {
                ExceptionController.AddException("Cannot inquire on primitive function '" + TokenText + "'. Inquiry can only be " +
                                                    "done to custom functions and variables.", TokenText.StartIndex, TokenText.EndIndex, 'P');
                return;
            }

            CustomFunction customFunction = (CustomFunction)SymbolTable.functions[TokenText.TokenText];
            
            int functionStartIndex = currentIndex;

            List<double> arguments = ParseArguments();
            // ParseArguments will move currentIndex past the TokenCategory.CloseBracket that ends this function

            if(arguments.Count != 0 && arguments.Count != customFunction.NumParameters())
            {
                ExceptionController.AddException("A function call can only be inquired if it has no arguments or the exact number of arguments " +
                    "the function requires. '" +  TokenText + "' requires " + customFunction.NumParameters + " arguments.", TokenText.StartIndex, TokenText.EndIndex, 'P');
            }

            if (executeFunctions)
            {
                List<Token> inquiryTokens = customFunction.Inquire(arguments);
                Tokens.RemoveRange(functionStartIndex, currentIndex - functionStartIndex + 1);  // Remove the function Tokens from the function to the TokenCategory.CloseBracket
                Tokens.Insert(functionStartIndex, new Token("(", TokenCategory.OpenParen, 0, 0));   // Add TokenCategory.OpenParen TokenText
                Tokens.InsertRange(functionStartIndex + 1, inquiryTokens);  // Add the inquiry Tokens
                currentIndex = functionStartIndex + inquiryTokens.Count + 1;  // Move currentIndex past the TokenCategory.OpenParen and inquiryTokens
                Tokens.Insert(currentIndex, new Token(")", TokenCategory.CloseParen, 0, 0));   // Add TokenCategory.CloseParen TokenText
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < Tokens.Count; x++)
            {
                if (x == currentIndex)
                {
                    sb.Append(" <" + Tokens[x].ToString() + "> ");
                }
                else
                {
                    sb.Append(Tokens[x] + " ");
                }
            }
            return sb.ToString();
        }
    }
}
