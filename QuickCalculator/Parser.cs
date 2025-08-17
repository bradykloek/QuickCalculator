using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickCalculator
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
        private double result;
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
            result = ParseExpression();
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

        private double ParseExpression()
        {
            if (tokens.Count() == 0) return 1;
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
                        if(left < 0)
                        {
                            left += right;  // C#'s % is actually remainder instead of modulo. This changes it to modulo behavior
                        }
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
            if (currentIndex >= tokens.Count())
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
                case 'n':
                    value = Double.Parse(tokens[currentIndex].GetToken());
                    break;
                case '(':
                    currentIndex++;    // Skip ( token
                    value = ParseExpression();
                    break;
                case 'v':
                case 'a':
                    if (localVariables.LocalsContains(token.GetToken()))
                    {   // Check if this variable is defined locally, since local variables should overshadow globals
                        value = localVariables.GetLocal(token.GetToken());
                    }
                    else if (SymbolTable.variables.Contains(token.GetToken()))
                    {
                        value = ((Variable)SymbolTable.variables[token.GetToken()]).GetValue();
                    }
                    else
                    {
                        ExceptionController.AddException("Undefined variable '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    }
                    break;
                case 'f':
                    value = ParseFunction();
                    break;
                case '?':
                    ParseInquiry();
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
            Function function = (Function)SymbolTable.functions[functionToken.GetToken()];

            List<double> arguments = PrepareArguments(functionToken);

            if (executeFunctions && ExceptionController.Count() == 0)
            {   // Only execute the function if this Evaluator is set to execute functions and there have been no exceptions
                return function.Execute(arguments);
            }
            else
            {   // Otherwise just return a dummy value of 1. The user won't see the result in this case anyway.
                return 1;
            }

            
        }

        /// <summary>
        /// Parses tokens starting at a function token until it reaches the ']' that terminates the function call.
        /// Each time an individual argument is terminated by a ',' token, it is parsed and added to the list of
        /// doubles that is eventually returned.
        /// </summary>
        /// <param name="functionToken"></param> The token of the function that is being parsed
        /// <returns></returns> A list of doubles representing the parsed arguments
        private List<double> ParseArguments(FunctionToken functionToken)
        {
            currentIndex += 2;  // Move past '['
            int level = functionToken.GetLevel();
            List<double> arguments = new List<double>();
            List<Token> argumentTokens = new List<Token>();
            while (currentIndex < tokens.Count())
            {   // End loop if we find a closing bracket of the same level as the function we are parsing
                if (tokens[currentIndex].GetCategory() == ']' && ((LevelToken)tokens[currentIndex]).GetLevel() == level) break;

                if (tokens[currentIndex].GetCategory() == ',' && ((LevelToken)tokens[currentIndex]).GetLevel() == level)
                {   // If we find a ',' of the same level as the current function, parse the tokens of the argument and add the resulting double to the functionToken's arguments list

                    Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                    // Parse this argument. Any errors will be added to the same evaluator

                    arguments.Add(parseArg.GetResult());
                    argumentTokens = new List<Token>();
                }
                else
                {   // Any token other than ',' comprises the argument, so add it to the argumentsTokens list
                    argumentTokens.Add(tokens[currentIndex]);
                }
                currentIndex++;
            }

            if (currentIndex >= tokens.Count())
            {   // If the loop didn't find a ']', there is an unmatched open bracket
                ExceptionController.AddException("Unmatched open bracket.", currentIndex - 1, currentIndex - 1, 'P');
                return null;
            }

            if (argumentTokens.Count() > 0)
            {   // Add the last argument if it has at least one token
                Parser parseArg = new Parser(argumentTokens, executeFunctions, localVariables);
                arguments.Add(parseArg.GetResult());
            }

            return arguments;
        }

        private List<double> PrepareArguments(FunctionToken functionToken)
        {
            Function function = (Function)SymbolTable.functions[functionToken.GetToken()];
            List<double> arguments = ParseArguments(functionToken);  // Populate arguments list

            if (arguments == null)
            {   // This has already added an exception inside of ParseArguments()
                return null;
            }

            if (!SymbolTable.functions.Contains(functionToken.GetToken()))
            {   // functionToken is not in the functions hash table
                ExceptionController.AddException("Undefined function '" + functionToken.GetToken() + "'.", functionToken.GetStart(), functionToken.GetEnd(), 'P');
                return null;
            }


            int expectedArgCount = function.GetNumArgs();
            int actualArgCount = arguments.Count();

            if (actualArgCount != expectedArgCount)
            {
                ExceptionController.AddException("Function '" + functionToken.GetToken() + "' requires " +
                                        expectedArgCount + " arguments, received " + actualArgCount + ".",
                                        functionToken.GetStart(), functionToken.GetEnd(), 'P');
                return null;
            }

            return arguments;
        }

        private void ParseInquiry()
        {
            if (currentIndex == tokens.Count() - 1)
            {
                ExceptionController.AddException("Inquiry operator '?' needs an expression to inquire.", currentIndex, currentIndex + 1, 'P');
                return;
            }
            if (tokens[currentIndex + 1].GetCategory() != 'v' && tokens[currentIndex + 1].GetCategory() != 'f')
            {
                ExceptionController.AddException("Inquiry operator '?' must immediately precede a variable or custom function", currentIndex, currentIndex + 1, 'P');
                return;
            }

            if (executeFunctions)
            {
                tokens.RemoveAt(currentIndex);  // Remove the '?' token
                if (tokens[currentIndex].GetCategory() == 'v') InquireVariable();
                else InquireFunction();
            }
        }

        private void InquireVariable()
        {
            Token token = tokens[currentIndex];
            tokens.RemoveAt(currentIndex);            // Remove the variable token
            if (!SymbolTable.variables.ContainsKey(token.GetToken()))
            {
                ExceptionController.AddException("Cannot inquire on undefined variable '" + token.GetToken() + "'.",
                                                    token.GetStart(), token.GetEnd(), 'P');
                return;
            }
            Variable variable = (Variable)SymbolTable.variables[token.GetToken()];
            List<Token> variableTokens = variable.GetTokens();
            InsertTokens(variableTokens);
        }

        private void InquireFunction()
        {
            Token token = tokens[currentIndex];

            if (!SymbolTable.functions.ContainsKey(token.GetToken()))
            {
                ExceptionController.AddException("Cannot inquire on undefined function '" + token.GetToken() + "'.",
                                                    token.GetStart(), token.GetEnd(), 'P');
                return;
            }
            if(!(SymbolTable.functions[token.GetToken()] is CustomFunction))
            {
                ExceptionController.AddException("Cannot inquire on primitive function '" + token.GetToken() + "'. Inquiry can only be " +
                                                    "done to custom functions and variables.", token.GetStart(), token.GetEnd(), 'P');
                return;
            }

            CustomFunction customFunction = (CustomFunction)SymbolTable.functions[token.GetToken()];

            if (tokens[currentIndex + 1].GetCategory() == '[' && tokens[currentIndex + 2].GetCategory() == ']')
            {   // Check if this function token has no arguments. If so, the user wants the raw function definition
                tokens.RemoveAt(currentIndex);
                InsertTokens(customFunction.GetTokens());
                return;
            }
            else
            {
                int startIndex = currentIndex;
                FunctionToken functionToken = (FunctionToken)token;
                int level = functionToken.GetLevel();
                List<double> arguments = PrepareArguments(functionToken);
                currentIndex = startIndex;      // Move currentIndex back to the function token
                tokens.RemoveAt(currentIndex);  // Remove the function token
                InsertTokens(customFunction.Inquire(arguments));

                currentIndex++;
                while (true)
                {
                    Token currentToken = tokens[currentIndex];
                    tokens.RemoveAt(currentIndex);
                    if(currentToken.GetCategory()==']' && ((LevelToken)currentToken).GetLevel() == level)
                    {
                        break;
                    }
                }
                return;
            }
        }

        private void InsertTokens(List<Token> newTokens)
        {
            tokens.Insert(currentIndex++, new Token("(", '(', 0, 0));       // Add '(' token
            for (int i = 0; i < newTokens.Count; i++)
            {
                tokens.Insert(currentIndex++, newTokens[i]);
            }
            tokens.Insert(currentIndex, new Token(")", ')', 0, 0));         // Add ')' token
            // Don't increment currentIndex because it will be incremented by ParsePrimary
        }




        public double GetResult()
        {
            return result;
        }

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < tokens.Count(); x++)
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
