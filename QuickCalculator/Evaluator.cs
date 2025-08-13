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
        private bool executeInput;
        private double result;
        private Tokenizer tokenizer;
        private Parser parser;
        private double roundPrecision;
        private string outputString;

        public Evaluator(string input, bool executeInput, double roundPrecision)
        {
            this.executeInput = executeInput;
            this.roundPrecision = roundPrecision;
            ExceptionController.ClearExceptions();

            tokenizer = new Tokenizer(input);

            if (tokenizer.GetAssignVariable() != "")
                PerformAssignment(tokenizer.GetAssignVariable());
            else if (tokenizer.GetDefineFunction() != null)
                DefineCustomFunction(tokenizer.GetDefineFunction());
            else if (ExceptionController.Count() == 0)
            {
                parser = new Parser(tokenizer.GetTokens(), executeInput);
                SetResult(parser.GetResult());
            }

        }

        private void PerformAssignment(string variableName)
        {
            Parser assignmentParser = new Parser(tokenizer.GetTokens(), executeInput);
            SetResult(assignmentParser.GetResult());
            if (ExceptionController.Count() == 0)
            {
                SymbolTable.variables[variableName] = result;
            }

            outputString = variableName + " = " + result;
        }


        private void DefineCustomFunction(CustomFunction customFunction)
        {
            List<Token> tokens = tokenizer.GetTokens();
            SymbolTable dummyParameters = customFunction.MarkParameters(tokens, customFunction != null);
            Parser definitionParser = new Parser(tokens, false, dummyParameters);
            // definitionParser is only used to check the syntax of the function definition-- its result doesn't mean anything

            if (executeInput)
            {   // Only if the user hit enter should we actually save this custom function
                customFunction.SetTokens(tokens);
                SymbolTable.functions[customFunction.GetName()] = customFunction;
            }

            outputString = customFunction.ToString();
        }

        private void SetResult(double value)
        {
            result = value;
            if (roundPrecision > 0 && Math.Abs(result - Math.Round(result)) <= roundPrecision)
            {
                result = Math.Round(value);
            }
            outputString = result.ToString();
        }


        public double GetResult()
        {
            return result;
        }
        public string ToString()
        {
            return outputString;
        }

        public List<Token> GetTokens()
        {
            return tokenizer.GetTokens();
        }
    }
}
