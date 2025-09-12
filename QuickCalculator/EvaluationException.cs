using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class EvaluationException : Exception
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
		public char Source { get; private set; }
		public EvaluationException() { }
        public EvaluationException(string message) : base(message) { }

        public EvaluationException(string message, int start, int end, char Source) : base(message)
        {
            StartIndex = start;
            EndIndex = end;
            this.Source = Source;
        }

        public override string ToString()
        {
            return "[" + StartIndex + "] " + base.Message + " (" + Source + ")";
        }

    }
}
