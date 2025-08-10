using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class Evaluator
    {
        private bool executeFunctions;
        private double result;
        private Tokenizer tokenizer;
        private Parser parser;
        private string assignVariable = "";
        private CustomFunction customFunction = null;

        public Evaluator(string input, bool executeFunctions, double roundPrecision)
        {
            this.executeFunctions = executeFunctions;
            ExceptionController.ClearExceptions();

            tokenizer = new Tokenizer(input, this);

            if (ExceptionController.Count() == 0)
            {
                if (customFunction == null)
                {
                    parser = new Parser(tokenizer.GetTokens(), executeFunctions);
                    result = parser.GetResult();
                }
                else
                {
                    // If customFunction isn't null then this input was defining a custom function

                    SymbolTable dummyParameters = customFunction.MarkParameters(tokenizer, customFunction != null);
                    parser = new Parser(tokenizer.GetTokens(), false, dummyParameters);
                    result = 0;     /* It doesn't matter what the parser reads here since this is just for defining a function
                                     * and the resulting value is irrelevant. (result will be 0 anyway) */

                    if (executeFunctions)
                    {   // Only if the user hit enter should we actually save this custom function
                        customFunction.SetTokens(tokenizer.GetTokens());
                        SymbolTable.functions[customFunction.GetName()] = customFunction;
                    }
                }
            }



            if (roundPrecision > 0 && Math.Abs(result - Math.Round(result)) <= roundPrecision)
            {
                result = Math.Round(result);
            }

        }


        public double GetResult()
        {
            return result;
        }

        public bool GetExecuteFunctions()
        {
            return executeFunctions;
        }

        public Tokenizer GetTokenizer()
        {
            return tokenizer;
        }

        public Parser GetParser()
        {
            return parser;
        }

        public string ToString()
        {
            return result.ToString();
        }

        public void SetAssignVariable(string variableName)
        {
            assignVariable = variableName;
        }

        public string GetAssignVariable()
        {
            return assignVariable;
        }

        public void SetCustomFunction(CustomFunction customFunction)
        {
            this.customFunction = customFunction;
        }

        public bool DefiningFunction()
        {
            return customFunction != null;
        }

        public CustomFunction GetUserFunction()
        {
            return customFunction;
        }
    }
}
