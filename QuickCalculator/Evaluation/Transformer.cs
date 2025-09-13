using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Transformer
    {
        public string AssignVariable { get; private set; } = "";
        // When a variable is being assigned, this will store the name of the variable
        public CustomFunction DefineFunction { get; private set; } = null;
        // When a function is being defined, this will store a CustomFunction object
        public bool IncludesInquiry { get; private set; } = false;
        // Stores whether an Inquiry is present

        private List<Token> tokens; // List that stores the tokens that will be passed in to the Validator

        public Transformer(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public void Transform()
        {
            int assignmentIndex = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                switch (token.category)
                {
                    case TokenCategory.Assignment:
                        if (assignmentIndex != -1)
                            ErrorController.AddError("Expression contains multiple assignment operators '='.", token.StartIndex, token.EndIndex, ErrorSource.Transformer);
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
                                                            " or Custom Function with zero arguments.", i, i + 1, ErrorSource.Transformer);
                        }
                        break;
                }
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
                ErrorController.AddError("Assignment needs two operands.", tokens[assignmentIndex].StartIndex, tokens[assignmentIndex].EndIndex, ErrorSource.Transformer);
                return;
            }

            if (assignmentIndex != 1 && assignmentIndex != tokens.Count - 2)    // Assignment token is not second or second to last           
            {
                // Check if this is an attempt to define a user function
                if (DefineCustomFunction(assignmentIndex)) return;

                ErrorController.AddError("Assignment can only be done to an isolated variable", tokens[assignmentIndex].StartIndex, tokens[assignmentIndex].EndIndex, ErrorSource.Transformer);
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
                    ErrorController.AddError("Cannot assign between two undefined variables.", prevToken.StartIndex, nextToken.EndIndex, ErrorSource.Transformer);
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

                    ErrorController.AddError("At least one operand must be a variable for assignment.", prevToken.StartIndex, nextToken.EndIndex, ErrorSource.Transformer);
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
                ErrorController.AddError("Cannot redefine primitive function '" + functionToken.TokenText + "'.", functionToken.StartIndex, assignmentIndex + 1, ErrorSource.Transformer);
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
                        ErrorController.AddError("Invalid token '" + token + "' in function signature.", token.StartIndex, token.EndIndex, ErrorSource.Transformer);
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
                ErrorController.AddError("Cannot inquire on undefined variable '" + varToken + "'.", varToken.StartIndex, varToken.EndIndex, ErrorSource.Transformer);
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
    }
}
