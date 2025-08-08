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
        private Tokenizer tokenizer;
        private List<Token> tokens;
        private int i;
        private double result;
        private Evaluator evaluator;


        /// <summary>
        /// Initializes a Parser using a completed Tokenizer object. This will parse and evaluate the tokens,
        /// and store the result in this.result.
        /// </summary>
        /// <param name="tokenizer"></param>
        public Parser(Tokenizer tokenizer, Evaluator evaluator)
        {
            this.evaluator = evaluator;
            this.tokenizer = tokenizer;
            tokens = tokenizer.GetTokens();
            i = 0;
            result = ParseExpression();
        }

        /// <summary>
        /// Checks that the current token (tokens[i]) is the same as matchWith. Avoids out of bounds errors.
        /// </summary>
        /// <param name="matchWith"></param> The string we are comparing against the current token
        /// <returns></returns>
        public bool MatchToken(string matchWith)
        {
            if (i >= tokens.Count) return false;
            return tokens[i].GetToken().Equals(matchWith);
        }

        private double ParseExpression()
        {
            if (tokenizer.GetTokens().Count() == 0) return 0;
            double left = ParseTerm();
            while (MatchToken("+") || MatchToken("-"))
            {
                string op = tokens[i].GetToken();
                i++; // Move past operator
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
                string op = tokens[i].GetToken();
                i++; // Move past operator
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
                        left = (int)left / (int)right;
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
                i++; // Move past operator
                double right = ParseFactor();
                left = Math.Pow(left, right);
            }
            return left;
        }

        private double ParsePrimary()
        {
            if (i >= tokens.Count())
            {
                // If we have gone out of bounds of the Token List, there was an operator at the end of the string which thus didn't have enough operands
                Token prevToken = tokens[i - 1];
                evaluator.AddException("Input ends with operator '" + prevToken.GetToken() + "', which does not have enough operands.", prevToken.GetStart(), prevToken.GetEnd());
                return 0;
            }
            Token token = tokens[i];
            double value = 0;
            switch (token.GetCategory())
            {
                case 'n':
                    value = Double.Parse(tokens[i].GetToken());
                    break;
                case '(':
                    i++;    // Skip ( token
                    value = ParseExpression();
                    break;
                default:
                    // If the switch falls to the default, there was a token we didn't account for.
                    evaluator.AddException("Encountered an unexpected token '" + token.GetToken() + "'.", token.GetStart(), token.GetEnd());
                    break;
            }
            i++;
            return value;
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
                if (x == i)
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
