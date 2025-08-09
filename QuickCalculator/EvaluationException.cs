using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class EvaluationException : Exception
    {
        private int startIndex;
        private int endIndex;
        private char source;
        public EvaluationException() { }
        public EvaluationException(string message) : base(message) { }

        public EvaluationException(string message, int start, int end, char source) : base(message)
        {
            startIndex = start;
            endIndex = end;
            this.source = source;
        }

        public int GetStart()
        {
            return startIndex;
        }

        public int GetEnd()
        {
            return endIndex;
        }

        public char GetSource()
        {
            return source;
        }
        public override string ToString()
        {
            return "[" + startIndex + "] " + base.Message + " (" + source + ")";
        }

    }
}
