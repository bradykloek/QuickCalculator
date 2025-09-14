namespace QuickCalculator.Tokens
{
    /// <summary>
    /// Represents an individual token of information. Tokenizer parses an input string to convert it into a list of Tokens.
    /// </summary>
    internal class Token
    {
        public string TokenText { get; set; } // String representing the contents of a single token
        public TokenCategory Category { get; set; }

        public bool Skip { get; set; }

        // Store the indexes of the input string that comprise this token. [start, end)
        public int StartIndex { get; private set; }
        public int EndIndex {get; private set;} 

        public Token(string token, TokenCategory category, int start, int end)
        {
            TokenText = token;
            this.Category = category;
            StartIndex = start;
            EndIndex = end;
            Skip = false;
        }

        public override string ToString()
        {
            return TokenText;
        }
    }
}
