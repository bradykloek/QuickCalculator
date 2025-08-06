using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Token
    {
        protected string token; // String representing an isolated token
        protected char type; /* Indicates the type of the token
                            *      s = Symbol (variable or function)
                            *      n = Number
                            *      o = Operator
                            */
        public Token(string token, char type)
        {
            this.token = token;
            this.type = type;
        }

        public override string ToString()
        {
            return token;
        }
    }
}
