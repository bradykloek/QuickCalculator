using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public CustomFunction(string name, List<Token> parameters)
        {
            this.name = name;
            this.parameters = parameters;
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

        public List<Token> GetParameters()
        {
            return parameters;
        }

        public SymbolTable MarkParameters(List<Token> tokens, bool definingFunction)
        {
            SymbolTable dummyParameters = new SymbolTable();
            for (int i = 0; i < parameters.Count(); i++)
            {
                for(int j = 0; j < tokens.Count(); j++)
                {
                    if (parameters[i].GetToken().Equals(tokens[j].GetToken()))
                    {   // If this parameter matches this token, update the token's category to be an argument 
                        tokens[j].SetCategory('a');
                        if (definingFunction)
                        {   /* We only add the local variables normally when the function is being executed (in this.Execute()). If the function
                             * is simply being defined currently, we'll need to populate the local variables with all of the parameters holding dummy values
                             * so the parser sees them as being defined. The value isn't relevant when we're defining a function has no real value anyway. */
                            dummyParameters.AddLocal(tokens[j].GetToken(), 0);
                        }
           
                    }
                }
            }
            return dummyParameters;
        }


        public override int GetNumArgs()
        {
            return parameters.Count();
        }
        public override double Execute(List<double> args)
        {
            if(tokens == null)
            {
                ExceptionController.AddException("Custom Function '" + name + "' not given definition tokens.", 0,0, 'F');
            }

            if(args.Count() != parameters.Count())
            {
                ExceptionController.AddException("Custom Function '" + name + "' expected " + parameters.Count() + 
                                        " arguments, received " + args.Count(), 0, 0, 'F');
                return 1;
            }

            Parser functionParser = new Parser(tokens, true, AssignVariables(args));
            return functionParser.GetResult();
        }

        public SymbolTable AssignVariables(List<double> args)
        {
            SymbolTable localVariables = new SymbolTable();
            // First we must assign all of the arguments to the correct parameters as local variables
            for (int i = 0; i < parameters.Count(); i++)
            {
                localVariables.AddLocal(parameters[i].GetToken(), args[i]);
            }
            return localVariables;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(name);
            sb.Append('[');

            for (int i = 0; i < parameters.Count(); i++)
            {
                sb.Append(parameters[i]);
                if(i < parameters.Count() - 1) sb.Append(',');
            }
            sb.Append(']');

            return sb.ToString();
        }
    }
}
