using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Parser
    {
        private List<Token> tokens;
        private int i;
        private double result;

        public double getResult()
        {
            return result;
        }

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int x = 0; x < tokens.Count(); x++)
            {
                if(x == i)
                {
                    sb.Append(" <"+tokens[x].ToString()+"> ");
                }
                else
                {
                    sb.Append(tokens[x]+" ");
                }
            }
            return sb.ToString();
        }

        public Parser(Tokenizer tokenizer)
        {
            tokens = tokenizer.GetTokens();
            i = 0;
            result = ParseExpression();
        }

        public bool MatchToken(string matchWith)
        {
            if (i >= tokens.Count) return false;
            Debug.WriteLine(i + " : " + tokens.Count);
            return tokens[i].GetToken().Equals(matchWith);
        }

        private double ParseExpression()
        {
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
            while (MatchToken("*") || MatchToken("/"))
            {
                string op = tokens[i].GetToken();
                i++; // Move past operator
                double right = ParseFactor();
                left = op.Equals("*") ? left * right : left / right;
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
                    i++;    // Skip ) token
                    break;
                default:
                    value = -999;
                    break;
            }
            i++;
            return value;
        }
    }
}
