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
        private List<Token> tokens;
        private string assignVariable = "";
        private CustomFunction defineFunction = null;
        bool includesInquiry = false;

        public Validator(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public string GetAssignVariable()
        {
            return assignVariable;
        }

        public CustomFunction GetDefineFunction()
        {
            return defineFunction;
        }

        public bool GetIncludesInquiry()
        {
            return includesInquiry;
        }

        public void Validate()
        {
            Stack<Token> parenStack = new Stack<Token>();
            int assignmentIndex = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                switch (token.GetToken())
                {
                    case "(":
                        parenStack.Push(token);
                        break;
                    case ")":
                        if (parenStack.Count > 0)
                            parenStack.Pop();
                        else ExceptionController.AddException("Unmatched closing parenthesis.", token.GetStart(), token.GetEnd(), 'V');
                        break;
                    case "=":
                        if (assignmentIndex != -1)
                            ExceptionController.AddException("Expression contains multiple assignment operators '='.", token.GetStart(), token.GetEnd(), 'V');
                        else
                        {
                            assignmentIndex = i;
                            PrepareAssignment(assignmentIndex);
                        }
                        break;
                    case "?":
                        includesInquiry = true;
                        if (i >= 1 && tokens[i - 1].GetCategory() == TokenCategory.Variable)
                        {
                            i = InquireVariable(i);
                        }
                        else if (i >= 3 && tokens[i - 1].GetCategory() == TokenCategory.CloseBracket)
                        {
                            InquireFunction(i);
                        }
                        else
                        {
                            ExceptionController.AddException("Inquiry operator '?' can only be used after a Variable, Custom Function with the correct number of arguments," +
                                                            " or Custom Function with zero arguments.", i, i + 1, 'V');
                        }
                        break;
                }

                while (parenStack.Count > 0)
                {
                    Token openParen = parenStack.Pop();
                    ExceptionController.AddException("Unmatched open parenthesis.", openParen.GetStart(), openParen.GetEnd(), 'P');
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
                ExceptionController.AddException("Assignment needs two operands.", tokens[assignmentIndex].GetStart(), tokens[assignmentIndex].GetEnd(), 'V');
                return;
            }

            if (assignmentIndex != 1 && assignmentIndex != tokens.Count - 2)    // Assignment token is not second or second to last           
            {
                // Check if this is an attempt to define a user function
                if (DefineCustomFunction(assignmentIndex)) return;

                ExceptionController.AddException("Assignment can only be done to an isolated variable", tokens[assignmentIndex].GetStart(), tokens[assignmentIndex].GetEnd(), 'V');
                return;
            }

            Token prevToken = tokens[assignmentIndex - 1];
            Token nextToken = tokens[assignmentIndex + 1];

            if (prevToken.GetCategory() == TokenCategory.Variable && nextToken.GetCategory() == TokenCategory.Variable) // If both are variables
            {
                if (SymbolTable.variables.ContainsKey(nextToken.GetToken()))
                {   // So long as next is defined, we assign prev <= next (leftward asignment by default)
                    variableIdx = assignmentIndex - 1;
                }
                else if (SymbolTable.variables.ContainsKey(prevToken.GetToken()))
                {   // If next isn't defined but prev is, then we do rightward assignment - prev => next
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Otherwise, neither are defined and we cannot successfully assign either
                    ExceptionController.AddException("Cannot assign between two undefined variables.", prevToken.GetStart(), nextToken.GetEnd(), 'V');
                    return;
                }

            }
            else
            {
                if (prevToken.GetCategory() == TokenCategory.Variable)
                {   // prev is the token that will receive the assignment
                    variableIdx = assignmentIndex - 1;
                }
                else if (nextToken.GetCategory() == TokenCategory.Variable)
                {   // next is the token that will receive the assignment
                    variableIdx = assignmentIndex + 1;
                }
                else
                {   // Neither operand is a variable

                    // Check if this is an attempt to define a user function
                    if (DefineCustomFunction(assignmentIndex)) return;

                    ExceptionController.AddException("At least one operand must be a variable for assignment", prevToken.GetStart(), nextToken.GetEnd(), 'V');
                    return;
                }
            }

            string variableName = tokens[variableIdx].GetToken();

            // Removing from the List will shift indices. We need to remove the lowest index twice to remove the variable and the assignment operator
            tokens.RemoveAt(Math.Min(assignmentIndex, variableIdx));
            tokens.RemoveAt(Math.Min(assignmentIndex, variableIdx));

            assignVariable = variableName;
        }


        private bool DefineCustomFunction(int assignmentIndex)
        {
            List<Token> parameters = new List<Token>();

            if (assignmentIndex < 3 || tokens[0].GetCategory() != TokenCategory.Function ||
                tokens[1].GetCategory() != TokenCategory.OpenBracket || tokens[assignmentIndex - 1].GetCategory() != TokenCategory.CloseBracket)
            {   // There must be at least three tokens before the assignment: TokenCategory.Function TokenCategory.OpenBracket TokenCategory.CloseBracket. Otherwise don't interpret this as a user function
                return false;
            }


            FunctionToken functionToken = (FunctionToken)tokens[0];
            if (SymbolTable.functions.ContainsKey(functionToken.GetToken())
                && SymbolTable.functions[functionToken.GetToken()] is PrimitiveFunction)
            {
                ExceptionController.AddException("Cannot redefine primitive function '" + functionToken.GetToken() + "'.", functionToken.GetStart(), assignmentIndex + 1, 'V');
                return false;
            }

            tokens.RemoveAt(0);     // Remove functionToken
            tokens.RemoveAt(0);     // Remove TokenCategory.OpenBracket

            int parameterCount = 0;
            while (tokens.Count > 0)
            {
                if (tokens[0].GetCategory() == TokenCategory.CloseBracket && functionToken.GetLevel() == ((LevelToken)tokens[0]).GetLevel())
                {   // Break the loop when we find the TokenCategory.CloseBracket that terminates this function token
                    break;
                }
                Token token = tokens[0];
                switch (token.GetCategory())
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
                        ExceptionController.AddException("Invalid token '" + token + "' in function signature.", token.GetStart(), token.GetEnd(), 'V');
                        return false;
                }
                tokens.RemoveAt(0);
            }
            tokens.RemoveAt(0);     // Remove TokenCategory.CloseBracket
            tokens.RemoveAt(0);     // Remove '='

            defineFunction = new CustomFunction(functionToken.GetToken(), parameters);
            return true;
        }

        private int InquireVariable(int inquiryIndex)
        {
            tokens.RemoveAt(inquiryIndex);
            Token varToken = tokens[inquiryIndex - 1];
            if (!SymbolTable.variables.ContainsKey(varToken.GetToken()))
            {
                ExceptionController.AddException("Cannot inquire on undefined variable '" + varToken + "'.", varToken.GetStart(), varToken.GetEnd(), 'V');
                return inquiryIndex;
            }

            Variable variable = SymbolTable.variables[tokens[tokens.Count - 1].GetToken()];
            // Insert parens in reverse order because they will be shifted by the later insertions
            tokens.Insert(inquiryIndex, new LevelToken(")", TokenCategory.CloseParen, 0, 0, 0));      
            tokens.InsertRange(inquiryIndex,variable.GetTokens());      // Add variable's tokens
            tokens.Add(new LevelToken("(", TokenCategory.OpenParen, 0, 0, 0));
            // The levels of the paren tokens don't matter here because they will not be seen by the user

            int index = inquiryIndex + variable.GetTokens().Count() + 1;
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
                if (tokens[i].GetCategory() == TokenCategory.Function && ((FunctionToken)tokens[i]).GetLevel() == closedBracket.GetLevel())
                {   // Searches for the function token that matches this closed bracket
                    break;
                }
            }
            tokens.Insert(i, new Token("?", TokenCategory.Inquiry, 0, 0));
        }
    }
}
