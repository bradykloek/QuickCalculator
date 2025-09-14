using System.Text;
using QuickCalculator.Errors;
using QuickCalculator.Symbols;
using QuickCalculator.Tokens;

namespace QuickCalculator.Evaluation
{
    internal class Evaluator
    {
        public List<Token> Tokens {  get; private set; } = new List<Token>();
        public bool ExecuteInput { get; private set; }
        public bool TemporaryResult { get; private set; }
        public string ResultString { get; private set; }

        private double roundPrecision;
        private double result;

        public Evaluator(bool ExecuteInput, double roundPrecision)
        {
            this.ExecuteInput = ExecuteInput;
            this.roundPrecision = roundPrecision;
            ErrorController.ClearErrors();
        }

        public void Evaluate(string input)
        {
            Tokenizer tokenizer = new Tokenizer(input);
            Tokens = tokenizer.Tokenize();

            if (ExecuteInput)
            {
                if (Tokens.Count > 0 && Tokens[0].Category == TokenCategory.Command)
                {
                    ExecuteCommand();
                    return;
                }
                if (tokenizer.IncludesInquiry)
                {
                    Inquirer inquirer = new Inquirer(Tokens);
                    inquirer.Inquire();
                    ResultString = TokenString();
                    return;
                }
            }

            Validator validator = new Validator(Tokens);
            validator.Validate();

            if (ErrorController.Count() != 0) return;

            if (validator.DefineFunction != null && ExecuteInput)
            {
                DefineCustomFunction(validator.DefineFunction);
                ResultString = "Function '" + validator.DefineFunction.Name + "' Defined";
                TemporaryResult = true;
                return;
            }

            Parser parser = new Parser(Tokens, ExecuteInput);
            SetResult(parser.ParseExpression());
            if (ExecuteInput)
            {
                SymbolTable.SetAns(result);
                if (validator.AssignVariable != "")
                {
                    PerformAssignment(validator.AssignVariable);
                    ResultString = validator.AssignVariable + " = " + result;
                    TemporaryResult = true;
                }
            }
  
        }

        private void PerformAssignment(string variableName)
        {
            SymbolTable.variables[variableName] = new Variable(result, Tokens);
        }


        private void DefineCustomFunction(CustomFunction customFunction)
        {
            customFunction.MarkParameters(Tokens);
            Parser definitionParser = new Parser(Tokens, false, customFunction.LocalVariables);
            definitionParser.ParseExpression();
            // definitionParser is only used to check the syntax of the function definition-- its result doesn't mean anything


            if (ExecuteInput)
            {   // Only if the user hit enter should we actually save this custom function
                customFunction.Tokens = Tokens;
                SymbolTable.functions[customFunction.Name] = customFunction;
            }
        }

        private void ExecuteCommand()
        {
            Command command = SymbolTable.commands[Tokens[0].TokenText];
            ResultString = command.Execute(Tokens.GetRange(1, Tokens.Count - 1));
            TemporaryResult = true;
        }


        private void SetResult(double value)
        {
            result = value;
            if (roundPrecision > 0 && Math.Abs(result - Math.Round(result)) <= roundPrecision)
            {
                result = Math.Round(result);
            }
            ResultString = result.ToString();
        }
        public string TokenString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < Tokens.Count; i++)
            {
                if (!Tokens[i].Skip)
                    sb.Append(Tokens[i].TokenText + " ");
            }
            return sb.ToString();
        }
    }
}
