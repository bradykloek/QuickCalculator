using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator.Symbols
{
    internal class Command
    {
        private TokenCategory[] parameters;
        public Command(TokenCategory[] parameters)
        {
            this.parameters = parameters;
        }

        public int NumParameters()
        {
            return parameters.Length;
        }

        public TokenCategory GetParameter(int index)
        {
            return parameters[index];
        }
    }
}
