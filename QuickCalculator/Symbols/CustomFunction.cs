using System.Text;
using QuickCalculator.Evaluation;
using QuickCalculator.Tokens;

namespace QuickCalculator.Symbols
{
    internal class CustomFunction : CalcFunction
    {
        public List<Token> Tokens { get; set; }
        public SymbolTable LocalVariables { get; private set; } = new SymbolTable();
        public string Name { get; private set; }

        private List<Token> parameters;
        public CustomFunction(string name, List<Token> parameters)
        {
            this.Name = name;
            this.parameters = parameters;
            DummyParameters();
        }

        public override int NumParameters()
        {
            return parameters.Count;
        }

        private void DummyParameters()
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                LocalVariables.AddLocal(parameters[i].TokenText, 0);
            }
        }

        
        /// <summary>
        /// Updates the category of Tokens that are in the parameters list to indicate they are arguments. 
        /// If this input is defining the function, this also adds the parameters as local variables with dummy values
        /// so the parser doesn't throw an exception for undefined variables.
        /// </summary>
        /// <param Name="Tokens"></param>   The list of all Tokens from the tokenizer
        /// <param Name="definingFunction"></param> Whether or not we are currently defining a function
        /// <returns></returns> 
        /// If we are defining a function, returns a SymbolTable that holds the parameters with dummy values.
        /// Otherwise it will return an empty SymbolTable that won't be used.
        public void MarkParameters(List<Token> Tokens)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                for(int j = 0; j < Tokens.Count; j++)
                {
                    if (parameters[i].TokenText.Equals(Tokens[j].TokenText))
                    {   // If this parameter matches this token, update the token's category to be an argument 
                        Tokens[j].category = TokenCategory.Parameter;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the custom function
        /// </summary>
        /// <param Name="args"></param> List of arguments that got passed in to this functoin
        /// <returns></returns>
        public override double Execute(List<double> args)
        {
            PairArguments(args);
            if (LocalVariables != null)
            {
                Parser functionParser = new Parser(Tokens, true, LocalVariables);
                return functionParser.ParseExpression();
            }
            return 1;
        }

        /// <summary>
        /// Returns a list representing the Tokens of this function when the parameters are replaced by the values of the arguments
        /// </summary>
        /// <param Name="args"></param> The arguments which are already parsed into doubles
        /// <returns></returns> The resulting Token List of the inquiry
        public List<Token> Inquire(List<double> args)
        {
            if(args.Count == 0)
            {   // If a function call with no parameters is inquired, we interpret it as the user inquiring on the raw Tokens that defined the function
                return Tokens;
            }

            PairArguments(args);
            List<Token> inquiryTokens = new List<Token>();
            for(int i = 0; i < Tokens.Count; i++)
            {
                Token token = Tokens[i];
                if(token.category == TokenCategory.Parameter)
                {   // If we are on an argument, add a new token that represents its numerical value
                    inquiryTokens.Add(new Token(LocalVariables.GetLocal(token.TokenText).ToString(),TokenCategory.Number,0,0));
                }
                else
                {   // Otherwise add the raw token
                    inquiryTokens.Add(Tokens[i]);
                }
            }
            return inquiryTokens;
        }

        /// <summary>
        /// Locally assigns the parameters to the passed in arguments.
        /// </summary>
        /// <param Name="args"></param> List of argument values
        /// <returns></returns> Returns a SymbolTable representing the local assignments
        private void PairArguments(List<double> args)
        {
            if (Tokens == null)
            {
                ExceptionController.AddException("Custom Function '" + Name + "' not given definition Tokens.", 0, 0, 'F');
                return;
            }

            if (args.Count != parameters.Count)
            {
                ExceptionController.AddException("Custom Function '" + Name + "' expected " + parameters.Count +
                                        " arguments, received " + args.Count, 0, 0, 'F');
                return;
            }

            // First we must assign all of the arguments to the correct parameters as local variables
            for (int i = 0; i < parameters.Count; i++)
            {
                LocalVariables.AddLocal(parameters[i].TokenText, args[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Name);
            sb.Append(TokenCategory.OpenBracket);

            for (int i = 0; i < parameters.Count; i++)
            {
                sb.Append(parameters[i]);
                if(i < parameters.Count - 1) sb.Append(',');
            }
            sb.Append(TokenCategory.CloseBracket);

            return sb.ToString();
        }
    }
}
