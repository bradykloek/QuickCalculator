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
        public double value { get; private set; }
        public List<Token> Tokens { get; private set; }

        public Variable(double value, List<Token> Tokens)
        {
            this.value = value;
            this.Tokens = Tokens;
        }

        public Variable(double value)
        {
            this.value = value;
            Tokens = new List<Token>();
            Tokens.Add(new Token(value.ToString(), TokenCategory.Number, 0, 0));
        }
    }
}
