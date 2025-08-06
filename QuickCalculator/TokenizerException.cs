using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class TokenizerException : Exception
    {
        private int charIndex = 0;
        public TokenizerException() { }
        public TokenizerException(string message) : base(message) { }

        public TokenizerException(string message, int charIndex) : base(message)
        {
            this.charIndex = charIndex;
        }

        public int CharIndex()
        {
            return charIndex;
        }

    }
}
