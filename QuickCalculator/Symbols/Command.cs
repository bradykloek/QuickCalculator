using QuickCalculator.Tokens;

namespace QuickCalculator.Symbols
{
    internal class Command
    {
        private TokenCategory[] parameters;
        private Func<List<Token>, string> function;
        public Command(Func<List<Token>, string> function, TokenCategory[] parameters)
        {
            this.parameters = parameters;
            this.function = function;
        }

        public Command(Func<List<Token>, string> function)
            : this(function, Array.Empty<TokenCategory>()) { }

        public int NumParameters()
        {
            return parameters.Length;
        }

        public TokenCategory GetParameter(int index)
        {
            return parameters[index];
        }

        public string Execute(List<Token> args)
        {
            return function(args);
        }
    }
}
