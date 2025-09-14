using System.Reflection.Metadata;
using System.Text;
using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Inquirer
    {

        private List<Token> tokens;

        public Inquirer(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public void Inquire()
        {
            for(int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Category == TokenCategory.Inquiry)
                {
                    tokens[i].Skip = true;
                    if (i >= 1 && tokens[i - 1].Category == TokenCategory.Variable)
                    {
                        InquireVariable(i);
                    }
                    else if (i >= 3 && tokens[i - 1].Category == TokenCategory.CloseBracket)
                    {
                        InquireFunction(i);
                    }
                    else
                    {
                        ErrorController.AddError("Inquiry operator '?' can only be used after a Variable, Custom Function with the correct number of arguments," +
                                                        " or Custom Function with zero arguments.", i, i + 1, ErrorSource.Inquirer);
                    }
                }
            }
        }

        public void InquireVariable(int inquiryIndex)
        {
            Token varToken = tokens[inquiryIndex - 1];
            if (!SymbolTable.variables.ContainsKey(varToken.TokenText))
            {
                ErrorController.AddError("Cannot inquire on undefined variable '" + varToken + "'.", varToken.StartIndex, varToken.EndIndex, ErrorSource.Inquirer);
            }

            Variable variable = SymbolTable.variables[varToken.TokenText];
            tokens[inquiryIndex - 1].TokenText = variable.InquiryString();
        }

        /// <summary>
        /// Replaces an inquired function token with its Source tokens, with any parameters replaced with the associated
        /// parsed argument values.
        /// </summary>
        private void InquireFunction(int inquiryIndex)
        {

            int functionIndex = FindFunctionIndex(inquiryIndex);
            Token functionToken = tokens[functionIndex];

            if (!SymbolTable.functions.ContainsKey(functionToken.TokenText))
            {
                ErrorController.AddError("Cannot inquire on undefined function '" + functionToken + "'.",
                                                    functionToken.StartIndex, functionToken.EndIndex, ErrorSource.Inquirer);
                return;
            }
            if (SymbolTable.functions[functionToken.TokenText] is PrimitiveFunction)
            {
                ErrorController.AddError("Cannot inquire on primitive function '" + functionToken + "'. Inquiry can only be " +
                                                    "done to custom functions and variables.", functionToken.StartIndex, functionToken.EndIndex, ErrorSource.Inquirer);
                return;
            }

            CustomFunction customFunction = (CustomFunction)SymbolTable.functions[functionToken.TokenText];


            List<object> arguments = ReadArguments(functionIndex, inquiryIndex);

            if(arguments.Count == 0)
            {
                tokens[functionIndex].TokenText = string.Join(" ", customFunction.Tokens);
            }
            else if (arguments.Count == customFunction.NumParameters())
            {
                SymbolTable pairedArguments = customFunction.PairArguments(arguments);
                List<Token> functionTokens = customFunction.Tokens;
                StringBuilder inquiryString = new StringBuilder();
                for (int i = 0; i < functionTokens.Count; i++)
                {
                    if (pairedArguments.LocalsContains(functionTokens[i].TokenText))
                    {
                        inquiryString.Append(pairedArguments.GetLocal(functionTokens[i].TokenText) + " ");
                    }
                    else
                    {
                        inquiryString.Append(functionTokens[i].TokenText + " ");
                    }
                }
                tokens[functionIndex].TokenText = inquiryString.ToString();
            }
            else
            {
                ErrorController.AddError("A function call can only be inquired if it has no arguments or the exact number of arguments " +
                    "the function requires. '" + functionToken + "' requires " + customFunction.NumParameters() + " arguments.", functionToken.StartIndex, functionToken.EndIndex, ErrorSource.Inquirer);
            }
        }

        private int FindFunctionIndex(int inquiryIndex)
        {
            LevelToken closedBracket = (LevelToken)tokens[inquiryIndex - 1];
            for (int i = inquiryIndex; i >= 0; i--)
            {
                if (tokens[i].Category == TokenCategory.Function && ((FunctionToken)tokens[i]).Level == closedBracket.Level)
                {   // Searches for the function token that matches this closed bracket
                    return i;
                }
                tokens[i].Skip = true;
            }
            return -1;
        }

        private List<object> ReadArguments(int functionIndex, int inquiryIndex)
        {
            FunctionToken functionToken = (FunctionToken)tokens[functionIndex];

            List<object> arguments = new List<object>();
            List<Token> argumentTokens = new List<Token>();

            for(int i = functionIndex + 2; i < inquiryIndex - 1; i++)
            {   // Loops through everything between [ and ]
                if (tokens[i].Category == TokenCategory.Comma && ((LevelToken)tokens[i]).Level == functionToken.Level)
                {
                    arguments.Add("( "+ string.Join(" ",argumentTokens) + " )");
                    argumentTokens.Clear();
                }
                else
                {
                    argumentTokens.Add(tokens[i]);
                }
            }
            if (argumentTokens.Count > 0)
            {   // Add the last argument if it has at least one token
                arguments.Add("( " + string.Join(" ", argumentTokens) + " )");
            }

            return arguments;
        }

    }
}
