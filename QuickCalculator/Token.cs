using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Token
    {
        protected string token; // String representing the contents of a single token
        protected char category; /* Indicates the category of the token
                                    *      s = Symbol (variable or function)
                                    *      n = Number
                                    *      o = Operator
                                    */

        public Token(string token, char category)
        {
            this.token = token;
            this.category = category;
        }

        public override string ToString()
        {
            return token;
        }

        public string GetToken()
        {
            return token;
        }

        public char GetCategory()
        {
            return category;
        }
    }
}
