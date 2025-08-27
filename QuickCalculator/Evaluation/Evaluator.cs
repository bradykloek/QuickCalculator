using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Evaluator
    {
        private bool executeInput;
        private double result;
        private Tokenizer tokenizer;
        private double roundPrecision;
        private string resultString;


        public Evaluator(bool executeInput, double roundPrecision)
        {
            this.executeInput = executeInput;
            this.roundPrecision = roundPrecision;
            ExceptionController.ClearExceptions();
        }

        public void Evaluate(string input)
        {
            tokenizer = new Tokenizer(input);

            if (input.Length == 0) return;

            tokenizer.Tokenize();

            Validator validator = new Validator(tokenizer.GetTokens());
            validator.Validate();


            if (validator.GetDefineFunction() != null)
            {
                DefineCustomFunction(validator.GetDefineFunction());
                resultString = input;
            }

            else if (ExceptionController.GetCount() == 0)
            {
                Parser parser = new Parser(tokenizer.GetTokens(), executeInput);
                SetResult(parser.ParseExpression());



                if (executeInput && validator.GetIncludesInquiry())
                {
                    resultString = TokenString();
                }

                if (executeInput && validator.GetAssignVariable() != "")
                {
                    if(!validator.GetIncludesInquiry())
                        PerformAssignment(validator.GetAssignVariable());
                    resultString = validator.GetAssignVariable() + " = " + result;
                }
            }
        }

        private void PerformAssignment(string variableName)
        {
            SymbolTable.variables[variableName] = new Variable(result, tokenizer.GetTokens());
        }


        private void DefineCustomFunction(CustomFunction customFunction)
        {
            List<Token> tokens = tokenizer.GetTokens();
            customFunction.MarkParameters(tokens);
            Parser definitionParser = new Parser(tokens, false, customFunction.GetLocals());
            definitionParser.ParseExpression();
            // definitionParser is only used to check the syntax of the function definition-- its result doesn't mean anything


            if (executeInput)
            {   // Only if the user hit enter should we actually save this custom function
                customFunction.SetTokens(tokens);
                SymbolTable.functions[customFunction.GetName()] = customFunction;
            }
        }

        private void SetResult(double value)
        {
            result = value;
            if (roundPrecision > 0 && Math.Abs(result - Math.Round(result)) <= roundPrecision)
            {
                result = Math.Round(value);
            }
            resultString = result.ToString();
        }

        public string Result()
        {
            return resultString;
        }

        public string TokenString()
        {
            List<Token> tokens = tokenizer.GetTokens();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tokens.Count; i++)
            {
                sb.Append(tokens[i] + " ");
            }
            return sb.ToString();
        }

        public List<Token> GetTokens()
        {
            return tokenizer.GetTokens();
        }
    }
}
