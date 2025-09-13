using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Validator
    {
        public string AssignVariable { get; private set; } = "";
        // When a variable is being assigned, this will store the name of the variable
        public CustomFunction DefineFunction { get; private set; } = null;
        // When a function is being defined, this will store a CustomFunction object
        public bool IncludesInquiry { get; private set; } = false;
        /* This stores whether an inquiry (using the '?' operator) has been completed within the Validator.
         * Variable inquiries can be completed and do not need to subsequently be parsed, but Function inquiries cannot
         * be completed at the Validator level so they must be saved for the Parser.
         */

        private List<Token> tokens; // List that stores the tokens that will be passed in to the Validator

        public Validator(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public void Validate()
        {
            Stack<Token> parenStack = new Stack<Token>();
            int assignmentIndex = -1;

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
                    case TokenCategory.Assignment:
                        if (assignmentIndex != -1)
                            ErrorController.AddError("Expression contains multiple assignment operators '='.", token.StartIndex, token.EndIndex, ErrorSource.Validator);
                        else
                        {
                            assignmentIndex = i;
                            PrepareAssignment(assignmentIndex);
                        }
                        break;
                    case TokenCategory.Inquiry:
                        IncludesInquiry = true;
                        if (i >= 1 && tokens[i - 1].category == TokenCategory.Variable)
                        {
                            i = InquireVariable(i);
                        }
                        else if (i >= 3 && tokens[i - 1].category == TokenCategory.CloseBracket)
                        {
                            // completedInquiry is not set to true because function inquiries need to be completed in the parser
                            InquireFunction(i);
                        }
                        else
                        {
                            ErrorController.AddError("Inquiry operator '?' can only be used after a Variable, Custom Function with the correct number of arguments," +
                                                            " or Custom Function with zero arguments.", i, i + 1, ErrorSource.Validator);
                        }
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

        /// <summary>
        /// After tokenization is completed, if an assignment operator was encountered this method will be called.
        /// Determines which token is the variable that will store the resulting value. This token is sent to be stored in
        /// evaluator then is removed from the token list.
        /// </summary>
        private void PrepareAssignment(int assignmentIndex)
        {
            int variableIdx;

            if (assignmentIndex == 0 || assignmentIndex == tokens.Count - 1)
            {   // Assignment operator is the first or last token
                ErrorController.AddError("Assignment needs two operands.", tokens[assignmentIndex].StartIndex, tokens[assignmentIndex].EndIndex, ErrorSource.Validator);
                return;
            }

            if (assignmentIndex != 1 && assignmentIndex != tokens.Count - 2)    // Assignment token is not second or second to last           
            {
                // Check if this is an attempt to define a user function
                if (DefineCustomFunction(assignmentIndex)) return;

                ErrorController.AddError("Assignment can only be done to an isolated variable", tokens[assignmentIndex].StartIndex, tokens[assignmentIndex].EndIndex, ErrorSource.Validator);
                return;
            }

            Token prevToken = tokens[assignmentIndex - 1];
            Token nextToken = tokens[assignmentIndex + 1];

            if (prevToken.category == TokenCategory.Variable && nextToken.category == TokenCategory.Variable) // If both are variables
            {
                if (SymbolTable.variables.ContainsKey(nextToken.TokenText))
                {   // So long as next is defined, we assign prev <= next (leftward assignment by default)
                    variableIdx = assignmentIndex - 1;
                }
                else if (SymbolTable.variables.ContainsKey(prevToken.TokenText))
                {   // If next isn't defined but prev is, then we do rightward assignment - prev => next
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Otherwise, neither are defined and we cannot successfully assign either
                    ErrorController.AddError("Cannot assign between two undefined variables.", prevToken.StartIndex, nextToken.EndIndex, ErrorSource.Validator);
                    return;
                }

            }
            else
            {
                if (prevToken.category == TokenCategory.Variable)
                {   // prev is the token that will receive the assignment
                    variableIdx = assignmentIndex - 1;
                }
                else if (nextToken.category == TokenCategory.Variable)
                {   // next is the token that will receive the assignment
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Neither operand is a variable

                    // Check if this is an attempt to define a user function
                    if (DefineCustomFunction(assignmentIndex)) return;

                    ErrorController.AddError("At least one operand must be a variable for assignment.", prevToken.StartIndex, nextToken.EndIndex, ErrorSource.Validator);
                    return;
                }
            }

            string variableName = tokens[variableIdx].TokenText;

            // Remove the variable and the assignment operator
            tokens.RemoveRange(Math.Min(assignmentIndex, variableIdx), 2);

            AssignVariable = variableName;
        }


        private bool DefineCustomFunction(int assignmentIndex)
        {
            List<Token> parameters = new List<Token>();

            if (assignmentIndex < 3 || tokens[0].category != TokenCategory.Function ||
                tokens[1].category != TokenCategory.OpenBracket || tokens[assignmentIndex - 1].category != TokenCategory.CloseBracket)
            {   // There must be at least three tokens before the assignment: TokenCategory.Function TokenCategory.OpenBracket TokenCategory.CloseBracket. Otherwise don't interpret this as a user function
                return false;
            }


            FunctionToken functionToken = (FunctionToken)tokens[0];
            if (SymbolTable.functions.ContainsKey(functionToken.TokenText)
                && SymbolTable.functions[functionToken.TokenText] is PrimitiveFunction)
            {
                ErrorController.AddError("Cannot redefine primitive function '" + functionToken.TokenText + "'.", functionToken.StartIndex, assignmentIndex + 1, ErrorSource.Validator);
                return false;
            }

            int index = 2;  // Start at index 2 to skip over the function and [
            int parameterCount = 0;
            for(; index < tokens.Count - 1; index++)
            {
                if (tokens[index].category == TokenCategory.CloseBracket && functionToken.Level == ((LevelToken)tokens[index]).Level)
                {   // Break the loop when we find the ] that terminates this function token
                    break;
                }
                Token token = tokens[index];
                switch (token.category)
                {
                    case TokenCategory.Variable:
                        // If it is a variable, add it to the local variables so when we parse the function definition we see that the parameters are defined
                        parameterCount++;
                        parameters.Add(token);
                        break;
                    case TokenCategory.Comma:
                        // Skip over the commas
                        break;
                    default:
                        ErrorController.AddError("Invalid token '" + token + "' in function signature.", token.StartIndex, token.EndIndex, ErrorSource.Validator);
                        return false;
                }
            }

            tokens.RemoveRange(0, index + 2);   // Remove the tokens from the function up to (including) the assignment operator

            DefineFunction = new CustomFunction(functionToken.TokenText, parameters);
            return true;
        }

        private int InquireVariable(int inquiryIndex)
        {
            tokens.RemoveAt(inquiryIndex);
            Token varToken = tokens[inquiryIndex - 1];
            if (!SymbolTable.variables.ContainsKey(varToken.TokenText))
            {
                ErrorController.AddError("Cannot inquire on undefined variable '" + varToken + "'.", varToken.StartIndex, varToken.EndIndex, ErrorSource.Validator);
                return inquiryIndex;
            }

            Variable variable = SymbolTable.variables[varToken.TokenText];
            // Insert parens in reverse order because they will be shifted by the later insertions
            tokens.Insert(inquiryIndex - 1, new LevelToken(")", TokenCategory.CloseParen, 0, 0, 0));
            tokens.InsertRange(inquiryIndex - 1, variable.Tokens);      // Add variable's tokens
            tokens.Insert(inquiryIndex - 1, new LevelToken("(", TokenCategory.OpenParen, 0, 0, 0));
            // The levels of the paren tokens don't matter here because they will not be seen by the user

            int index = inquiryIndex + variable.Tokens.Count + 1;
            tokens.RemoveAt(index);
            return index - 1;
        }

        private void InquireFunction(int inquiryIndex)
        {
            tokens.RemoveAt(inquiryIndex);
            LevelToken closedBracket = (LevelToken)tokens[inquiryIndex - 1];
            int i;
            for (i = inquiryIndex - 2; i >= 0; i--)
            {
                if (tokens[i].category == TokenCategory.Function && ((FunctionToken)tokens[i]).Level == closedBracket.Level)
                {   // Searches for the function token that matches this closed bracket
                    break;
                }
            }
            tokens.Insert(i, new Token("?", TokenCategory.Inquiry, 0, 0));
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
