using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool CompletedInquiry { get; private set; } = false;
        /* This stores whether an inquiry (using the '?' operator) has been completed within the Validator.
         * Variable inquiries can be completed and do not need to subsequently be parsed, but Function inquiries cannot
         * be completed at the Validator level so they must be saved for the Parser.
         */

        private List<Token> Tokens; // List that stores the Tokens that will be passed in to the Validator

        public Validator(List<Token> Tokens)
        {
            this.Tokens = Tokens;
        }

        public void Validate()
        {
            Stack<Token> parenStack = new Stack<Token>();
            int assignmentIndex = -1;

            for (int i = 0; i < Tokens.Count; i++)
            {
                Token TokenText = Tokens[i];
                switch (TokenText.category)
                {
                    case TokenCategory.OpenParen:
                        parenStack.Push(TokenText);
                        break;
                    case TokenCategory.CloseParen:
                        if (parenStack.Count > 0)
                            parenStack.Pop();
                        else ExceptionController.AddException("Unmatched closing parenthesis.", TokenText.StartIndex, TokenText.EndIndex, 'V');
                        break;
                    case TokenCategory.Assignment:
                        if (assignmentIndex != -1)
                            ExceptionController.AddException("Expression contains multiple assignment operators '='.", TokenText.StartIndex, TokenText.EndIndex, 'V');
                        else
                        {
                            assignmentIndex = i;
                            PrepareAssignment(assignmentIndex);
                        }
                        break;
                    case TokenCategory.Inquiry:
                        if (i >= 1 && Tokens[i - 1].category == TokenCategory.Variable)
                        {
                            CompletedInquiry = true;
                            i = InquireVariable(i);
                        }
                        else if (i >= 3 && Tokens[i - 1].category == TokenCategory.CloseBracket)
                        {
                            // completedInquiry is not set to true because function inquiries need to be completed in the parser
                            InquireFunction(i);
                        }
                        else
                        {
                            ExceptionController.AddException("Inquiry operator '?' can only be used after a Variable, Custom Function with the correct number of arguments," +
                                                            " or Custom Function with zero arguments.", i, i + 1, 'V');
                        }
                        break;
                    case TokenCategory.Command:
                        if (i != 0)
                            ExceptionController.AddException("Command can only be at the start of the input.", TokenText.StartIndex, TokenText.EndIndex, 'V');
                        else
                            ValidateCommand(TokenText, i);
                        break;
                }
            }

            while (parenStack.Count > 0)
            {
                Token openParen = parenStack.Pop();
                ExceptionController.AddException("Unmatched open parenthesis.", openParen.StartIndex, openParen.EndIndex, 'V');
            }
        }

        /// <summary>
        /// After TokenTextization is completed, if an assignment operator was encountered this method will be called.
        /// Determines which TokenText is the variable that will store the resulting value. This TokenText is sent to be stored in
        /// evaluator then is removed from the TokenText list.
        /// </summary>
        private void PrepareAssignment(int assignmentIndex)
        {
            int variableIdx;

            if (assignmentIndex == 0 || assignmentIndex == Tokens.Count - 1)
            {   // Assignment operator is the first or last TokenText
                ExceptionController.AddException("Assignment needs two operands.", Tokens[assignmentIndex].StartIndex, Tokens[assignmentIndex].EndIndex, 'V');
                return;
            }

            if (assignmentIndex != 1 && assignmentIndex != Tokens.Count - 2)    // Assignment TokenText is not second or second to last           
            {
                // Check if this is an attempt to define a user function
                if (DefineCustomFunction(assignmentIndex)) return;

                ExceptionController.AddException("Assignment can only be done to an isolated variable", Tokens[assignmentIndex].StartIndex, Tokens[assignmentIndex].EndIndex, 'V');
                return;
            }

            Token prevToken = Tokens[assignmentIndex - 1];
            Token nextToken = Tokens[assignmentIndex + 1];

            if (prevToken.category == TokenCategory.Variable && nextToken.category == TokenCategory.Variable) // If both are variables
            {
                if (SymbolTable.variables.ContainsKey(nextToken.TokenText))
                {   // So long as next is defined, we assign prev <= next (leftward asignment by default)
                    variableIdx = assignmentIndex - 1;
                }
                else if (SymbolTable.variables.ContainsKey(prevToken.TokenText))
                {   // If next isn't defined but prev is, then we do rightward assignment - prev => next
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Otherwise, neither are defined and we cannot successfully assign either
                    ExceptionController.AddException("Cannot assign between two undefined variables.", prevToken.StartIndex, nextToken.EndIndex, 'V');
                    return;
                }

            }
            else
            {
                if (prevToken.category == TokenCategory.Variable)
                {   // prev is the TokenText that will receive the assignment
                    variableIdx = assignmentIndex - 1;
                }
                else if (nextToken.category == TokenCategory.Variable)
                {   // next is the TokenText that will receive the assignment
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Neither operand is a variable

                    // Check if this is an attempt to define a user function
                    if (DefineCustomFunction(assignmentIndex)) return;

                    ExceptionController.AddException("At least one operand must be a variable for assignment", prevToken.StartIndex, nextToken.EndIndex, 'V');
                    return;
                }
            }

            string variableName = Tokens[variableIdx].TokenText;

            // Removing from the List will shift indices. We need to remove the lowest index twice to remove the variable and the assignment operator
            Tokens.RemoveAt(Math.Min(assignmentIndex, variableIdx));
            Tokens.RemoveAt(Math.Min(assignmentIndex, variableIdx));

            AssignVariable = variableName;
        }


        private bool DefineCustomFunction(int assignmentIndex)
        {
            List<Token> parameters = new List<Token>();

            if (assignmentIndex < 3 || Tokens[0].category != TokenCategory.Function ||
                Tokens[1].category != TokenCategory.OpenBracket || Tokens[assignmentIndex - 1].category != TokenCategory.CloseBracket)
            {   // There must be at least three Tokens before the assignment: TokenCategory.Function TokenCategory.OpenBracket TokenCategory.CloseBracket. Otherwise don't interpret this as a user function
                return false;
            }


            FunctionToken functionToken = (FunctionToken)Tokens[0];
            if (SymbolTable.functions.ContainsKey(functionToken.TokenText)
                && SymbolTable.functions[functionToken.TokenText] is PrimitiveFunction)
            {
                ExceptionController.AddException("Cannot redefine primitive function '" + functionToken.TokenText + "'.", functionToken.StartIndex, assignmentIndex + 1, 'V');
                return false;
            }

            Tokens.RemoveAt(0);     // Remove functionToken
            Tokens.RemoveAt(0);     // Remove TokenCategory.OpenBracket

            int parameterCount = 0;
            while (Tokens.Count > 0)
            {
                if (Tokens[0].category == TokenCategory.CloseBracket && functionToken.Level == ((LevelToken)Tokens[0]).Level)
                {   // Break the loop when we find the TokenCategory.CloseBracket that terminates this function TokenText
                    break;
                }
                Token TokenText = Tokens[0];
                switch (TokenText.category)
                {
                    case TokenCategory.Variable:
                        // If it is a variable, add it to the local variables so when we parse the function definition we see that the parameters are defined
                        parameterCount++;
                        parameters.Add(TokenText);
                        break;
                    case TokenCategory.Comma:
                        // Skip over the commas
                        break;
                    default:
                        ExceptionController.AddException("Invalid TokenText '" + TokenText + "' in function signature.", TokenText.StartIndex, TokenText.EndIndex, 'V');
                        return false;
                }
                Tokens.RemoveAt(0);
            }
            Tokens.RemoveAt(0);     // Remove TokenCategory.CloseBracket
            Tokens.RemoveAt(0);     // Remove '='

            DefineFunction = new CustomFunction(functionToken.TokenText, parameters);
            return true;
        }

        private int InquireVariable(int inquiryIndex)
        {
            Tokens.RemoveAt(inquiryIndex);
            Token varToken = Tokens[inquiryIndex - 1];
            if (!SymbolTable.variables.ContainsKey(varToken.TokenText))
            {
                ExceptionController.AddException("Cannot inquire on undefined variable '" + varToken + "'.", varToken.StartIndex, varToken.EndIndex, 'V');
                return inquiryIndex;
            }

            Variable variable = SymbolTable.variables[Tokens[Tokens.Count - 1].TokenText];
            // Insert parens in reverse order because they will be shifted by the later insertions
            Tokens.Insert(inquiryIndex, new LevelToken(")", TokenCategory.CloseParen, 0, 0, 0));
            Tokens.InsertRange(inquiryIndex, variable.Tokens);      // Add variable's Tokens
            Tokens.Add(new LevelToken("(", TokenCategory.OpenParen, 0, 0, 0));
            // The levels of the paren Tokens don't matter here because they will not be seen by the user

            int index = inquiryIndex + variable.Tokens.Count + 1;
            Tokens.RemoveAt(index);
            CompletedInquiry = true;
            return index - 1;
        }

        private void InquireFunction(int inquiryIndex)
        {
            Tokens.RemoveAt(inquiryIndex);
            LevelToken closedBracket = (LevelToken)Tokens[inquiryIndex - 1];
            int i;
            for (i = inquiryIndex - 2; i >= 0; i--)
            {
                if (Tokens[i].category == TokenCategory.Function && ((FunctionToken)Tokens[i]).Level == closedBracket.Level)
                {   // Searches for the function TokenText that matches this closed bracket
                    break;
                }
            }
            Tokens.Insert(i, new Token("?", TokenCategory.Inquiry, 0, 0));
        }


        private void ValidateCommand(Token commandToken, int StartIndex)
        {
            if (StartIndex != 0)
            {
                ExceptionController.AddException("Command must be at the beginning of the input.", commandToken.StartIndex, commandToken.EndIndex, 'V');
                return;
            }

            if (!SymbolTable.commands.ContainsKey(commandToken.TokenText))
            {
                ExceptionController.AddException("Unknown command '" + commandToken + "'.", commandToken.StartIndex, commandToken.EndIndex, 'V');
                return;
            }

            Command command = SymbolTable.commands[commandToken.TokenText];

            if (Tokens.Count - 1 != command.NumParameters())
            {
                ExceptionController.AddException("Command '" + commandToken + "' requires " + command.NumParameters() + " arguments, received "
                                                    + (Tokens.Count - 1) + ".", commandToken.StartIndex, commandToken.EndIndex, 'V');
                return;
            }

            for (int i = 0; i < command.NumParameters(); i++)
            {
                if (Tokens[i + 1].category != command.GetParameter(i))
                {
                    ExceptionController.AddException("Argument '" + (i + 1) + " for command " + commandToken + " should a " + command.GetParameter(i) +
                                                     " TokenText, recieved " + Tokens[i + 1].category + ".", Tokens[i + 1].StartIndex, Tokens[i + 1].EndIndex, 'V');
                }
            }
        }
    }
}
