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
        public EvaluationException() { }
        public EvaluationException(string message) : base(message) { }

        public EvaluationException(string message, int start, int end) : base(message)
        {
            startIndex = start;
            endIndex = end;
        }

        public int GetStart()
        {
            return startIndex;
        }

        public int GetEnd()
        {
            return endIndex;
        }

        public override string ToString()
        {
            return "[" + startIndex + "] " + base.Message;
        }

    }
}
