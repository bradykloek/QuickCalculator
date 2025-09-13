namespace QuickCalculator.Errors
{
    internal class EvaluationError
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
		public ErrorSource Source { get; private set; }
        public string Message { get; private set; }
		public EvaluationError() { }
        public EvaluationError(string message)
        {
            Message = message;
        }

        public EvaluationError(string message, int start, int end, ErrorSource source)
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
