namespace QuickCalculator
{
    internal class EvaluationException
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
		public char Source { get; private set; }
        public string Message { get; private set; }
		public EvaluationException() { }
        public EvaluationException(string message)
        {
            Message = message;
        }

        public EvaluationException(string message, int start, int end, char source)
        {
            Message = message;
            StartIndex = start;
            EndIndex = end;
            Source = source;
        }

        public override string ToString()
        {
            return "[" + StartIndex + "] " + Message + " (" + Source + ")";
        }

    }
}
