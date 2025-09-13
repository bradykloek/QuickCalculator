using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Validator
    {

        private List<Token> tokens; // List that stores the tokens that will be passed in to the Validator

        public Validator(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public void Validate()
        {
            Stack<Token> parenStack = new Stack<Token>();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                switch (token.category)
                {
                    case TokenCategory.OpenParen:
                        parenStack.Push(token);
                        break;
                    case TokenCategory.CloseParen:
                        if (parenStack.Count > 0)
                            parenStack.Pop();
                        else ErrorController.AddError("Unmatched closing parenthesis.", token.StartIndex, token.EndIndex, ErrorSource.Validator);
                        break;
                    case TokenCategory.Command:
                        if (i != 0)
                            ErrorController.AddError("Command can only be at the start of the input.", token.StartIndex, token.EndIndex, ErrorSource.Validator);
                        else
                            ValidateCommand(token, i);
                        break;
                }
            }

            while (parenStack.Count > 0)
            {
                Token openParen = parenStack.Pop();
                ErrorController.AddError("Unmatched open parenthesis.", openParen.StartIndex, openParen.EndIndex, ErrorSource.Validator);
            }
        }



        private void ValidateCommand(Token commandToken, int StartIndex)
        {
            if (StartIndex != 0)
            {
                ErrorController.AddError("Command must be at the beginning of the input.", commandToken.StartIndex, commandToken.EndIndex, ErrorSource.Validator);
                return;
            }

            if (!SymbolTable.commands.ContainsKey(commandToken.TokenText))
            {
                ErrorController.AddError("Unknown command '" + commandToken + "'.", commandToken.StartIndex, commandToken.EndIndex, ErrorSource.Validator);
                return;
            }

            Command command = SymbolTable.commands[commandToken.TokenText];

            if (tokens.Count - 1 != command.NumParameters())
            {
                ErrorController.AddError("Command '" + commandToken + "' requires " + command.NumParameters() + " arguments, received "
                                                    + (tokens.Count - 1) + ".", commandToken.StartIndex, commandToken.EndIndex, ErrorSource.Validator);
                return;
            }

            for (int i = 0; i < command.NumParameters(); i++)
            {
                if (tokens[i + 1].category != command.GetParameter(i))
                {
                    ErrorController.AddError("Argument '" + (i + 1) + "' for command " + commandToken + " should be a " + command.GetParameter(i) +
                                                     " token, received " + tokens[i + 1].category + ".", tokens[i + 1].StartIndex, tokens[i + 1].EndIndex, ErrorSource.Validator);
                }
            }
        }
    }
}
