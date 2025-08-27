using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCalculator.Tokens;

namespace QuickCalculator.Symbols
{
    internal class Variable
    {
        private double value;
        private List<Token> tokens;

        public Variable(double value, List<Token> tokens)
        {
            this.value = value;
            this.tokens = tokens;
        }

        public Variable(double value)
        {
            this.value = value;
            tokens = new List<Token>();
            tokens.Add(new Token(value.ToString(), TokenCategory.Number, 0, 0));
        }

        public double GetValue()
        {
            return value;
        }

        public List<Token> GetTokens()
        {
            return tokens;
        }
    }
}
