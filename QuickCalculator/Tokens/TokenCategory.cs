namespace QuickCalculator.Tokens
{
    public enum TokenCategory : byte
    {
        Uncategorized,
        Number,
        Operator,
        Variable,
        Function,
        Parameter,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        Assignment,
        Comma,
        Negation,
        Inquiry,
        Command,
    }
}
