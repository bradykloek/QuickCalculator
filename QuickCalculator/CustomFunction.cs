using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuickCalculator
{
    internal class CustomFunction : Function
    {
        private string name;
        private List<Token> parameters;
        private List<Token> ? tokens;
        private SymbolTable localVariables = new SymbolTable();
        public CustomFunction(string name, List<Token> parameters)
        {
            this.name = name;
            this.parameters = parameters;
            DummyParameters();
        }

        private void DummyParameters()
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                localVariables.AddLocal(parameters[i].GetToken(), 0);
            }
        }

        public string GetName()
        {
            return name;
        }

        public List<Token> GetTokens()
        {
            return tokens;
        }
        public void SetTokens(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        
        /// <summary>
        /// Updates the category of tokens that are in the parameters list to indicate they are arguments. 
        /// If this input is defining the function, this also adds the parameters as local variables with dummy values
        /// so the parser doesn't throw an exception for undefined variables.
        /// </summary>
        /// <param name="tokens"></param>   The list of all tokens from the tokenizer
        /// <param name="definingFunction"></param> Whether or not we are currently defining a function
        /// <returns></returns> 
        /// If we are defining a function, returns a SymbolTable that holds the parameters with dummy values.
        /// Otherwise it will return an empty SymbolTable that won't be used.
        public void MarkParameters(List<Token> tokens)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                for(int j = 0; j < tokens.Count; j++)
                {
                    if (parameters[i].GetToken().Equals(tokens[j].GetToken()))
                    {   // If this parameter matches this token, update the token's category to be an argument 
                        tokens[j].SetCategory('a');
                    }
                }
            }
        }

        public override int GetNumParameters()
        {
            return parameters.Count;
        }

        public SymbolTable GetLocals()
        {
            return localVariables;
        }

        /// <summary>
        /// Executes the custom function
        /// </summary>
        /// <param name="args"></param> List of arguments that got passed in to this functoin
        /// <returns></returns>
        public override double Execute(List<double> args)
        {
            PairArguments(args);
            if (localVariables != null)
            {
                Parser functionParser = new Parser(tokens, true, localVariables);
                return functionParser.GetResult();
            }
            return 1;
        }

        /// <summary>
        /// Returns a list representing the tokens of this function when the parameters are replaced by the values of the arguments
        /// </summary>
        /// <param name="args"></param> The arguments which are already parsed into doubles
        /// <returns></returns> The resulting Token List of the inquiry
        public List<Token> Inquire(List<double> args)
        {
            if(args.Count == 0)
            {   // If a function call with no parameters is inquired, we interpret it as the user inquiring on the raw tokens that defined the function
                return tokens;
            }

            PairArguments(args);
            List<Token> inquiryTokens = new List<Token>();
            for(int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if(token.GetCategory() == 'a')
                {   // If we are on an argument, add a new token that represents its numerical value
                    inquiryTokens.Add(new Token(localVariables.GetLocal(token.GetToken()).ToString(),'n',0,0));
                }
                else
                {   // Otherwise add the raw token
                    inquiryTokens.Add(tokens[i]);
                }
            }
            return inquiryTokens;
        }

        /// <summary>
        /// Locally assigns the parameters to the passed in arguments.
        /// </summary>
        /// <param name="args"></param> List of argument values
        /// <returns></returns> Returns a SymbolTable representing the local assignments
        private void PairArguments(List<double> args)
        {
            if (tokens == null)
            {
                ExceptionController.AddException("Custom Function '" + name + "' not given definition tokens.", 0, 0, 'F');
                return;
            }

            if (args.Count != parameters.Count)
            {
                ExceptionController.AddException("Custom Function '" + name + "' expected " + parameters.Count +
                                        " arguments, received " + args.Count, 0, 0, 'F');
                return;
            }

            // First we must assign all of the arguments to the correct parameters as local variables
            for (int i = 0; i < parameters.Count; i++)
            {
                localVariables.AddLocal(parameters[i].GetToken(), args[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append('[');

            for (int i = 0; i < parameters.Count; i++)
            {
                sb.Append(parameters[i]);
                if(i < parameters.Count - 1) sb.Append(',');
            }
            sb.Append(']');

            return sb.ToString();
        }
    }
}
