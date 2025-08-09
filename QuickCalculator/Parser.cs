using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Evaluator evaluator;

        /// <summary>
        /// Initializes a Parser using a completed token list. This will parse and evaluate the tokens,
        /// and store the result in this.result.
        /// </summary>
        /// <param name="tokens"></param>
        public Parser(List<Token> tokens, Evaluator evaluator)
        {
            this.evaluator = evaluator;
            this.tokens = tokens;
            currentIndex = 0;
            result = ParseExpression();
        }

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
            if (tokens.Count() == 0) return 0;
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
                            left += right;  // C# % is actually remainder instead of modulo. This changes it to modulo behavior
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
            double left = ParsePrimary();
            while (MatchToken("^"))
            {
                currentIndex++; // Move past operator
                double right = ParseFactor();
                left = Math.Pow(left, right);
            }
            return left;
        }

        private double ParsePrimary()
        {
            if (currentIndex >= tokens.Count())
            {
                // If we have gone out of bounds of the Token List, there was an operator at the end of the string which thus didn't have enough operands
                Token prevToken = tokens[currentIndex - 1];
                evaluator.AddException("Input ends with operator '" + prevToken.GetToken() + "', which does not have enough operands.", prevToken.GetStart(), prevToken.GetEnd(), 'P');
                return 0;
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
                    if (Symbols.variables.Contains(token.GetToken()))
                    {
                        value = (double)Symbols.variables[token.GetToken()];
                    }
                    else
                    {
                        evaluator.AddException("Undefined variable '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    }
                    break;
                case 'f':
                    FunctionToken functionToken = (FunctionToken)tokens[currentIndex];
                    if (!ParseFunction(functionToken)) return 0;
                    if (!Symbols.functions.Contains(token.GetToken()))
                        evaluator.AddException("Undefined function '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    else
                    {
                        Function function = (Function)Symbols.functions[token.GetToken()];
                        List<double> args = functionToken.GetArgs();
                        int actualArgCount = args.Count();
                        int expectedArgCount = function.GetNumArgs();
                        if (actualArgCount != expectedArgCount)
                            evaluator.AddException( "Function '" + token.GetToken() + "' requires " + 
                                                    expectedArgCount + " arguments, received " + actualArgCount + ".",
                                                    functionToken.GetStart(), functionToken.GetEnd(), 'P');
                        else
                        {
                            value = evaluator.GetExecuteFunctions() ? function.Execute(args) : 1;
                        }

                    }
                    break;
                default:
                    // If the switch falls to the default, there was a token we didn't account for.
                    evaluator.AddException("Encountered an unexpected token '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd(), 'P');
                    break;
            }
            currentIndex++;
            return value;
        }

        private bool ParseFunction(FunctionToken functionToken)
        {
            currentIndex += 2;  // Move past '['
            int level = functionToken.GetLevel();
            List<Token> argumentTokens = new List<Token>();
            while (currentIndex < tokens.Count())
            {
                if (tokens[currentIndex].GetCategory() == ']' && ((LevelToken)tokens[currentIndex]).GetLevel() == level) break;

                if (tokens[currentIndex].GetCategory() == ',' && ((LevelToken)tokens[currentIndex]).GetLevel() == level)
                {
                    Parser parseArg = new Parser(argumentTokens, evaluator);
                    // Parse this argument. Any errors will be added to the same evaluator

                    functionToken.AddArg(parseArg.GetResult());
                    argumentTokens = new List<Token>();
                }
                else
                {
                    argumentTokens.Add(tokens[currentIndex]);
                }
                currentIndex++;
            }

            if (currentIndex >= tokens.Count())
            {
                evaluator.AddException("Unmatched open bracket.", currentIndex - 1, currentIndex - 1, 'P');
                return false;
            }

            if (argumentTokens.Count() > 0)
            {   // Add the last argument
                Parser parseArg = new Parser(argumentTokens, evaluator);
                functionToken.AddArg(parseArg.GetResult());
                currentIndex++;
            }

            return true;
        }


        public double GetResult()
        {
            return result;
        }

        public string DebugToString()
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

        public string ToString()
        {
            return result.ToString();
        }

    }
}
