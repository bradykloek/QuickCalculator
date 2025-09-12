using System.Text;
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
            ExceptionController.ClearExceptions();
        }

        public void Evaluate(string input)
        {
            Tokenizer t = new Tokenizer(input);
            Tokens = t.Tokenize();

            Validator v = new Validator(Tokens);
            v.Validate();

            if (ExceptionController.Count() != 0) return;

            if (Tokens.Count > 0 && Tokens[0].category == TokenCategory.Command)
            {
                if(ExecuteInput)
                    ExecuteCommand();
                return;
            }

            if (v.DefineFunction != null)
            {
                DefineCustomFunction(v.DefineFunction);
                ResultString = "Function '" + v.DefineFunction.Name + "' Defined";
                TemporaryResult = true;
                return;
            }

            Parser parser = new Parser(Tokens, ExecuteInput);
            SetResult(parser.ParseExpression());
            if (ExecuteInput)
            {
                SymbolTable.SetAns(result);
                if (v.AssignVariable != "")
                {
                    PerformAssignment(v.AssignVariable);
                    ResultString = v.AssignVariable + " = " + result;
                    TemporaryResult = true;
                }

                if(v.IncludesInquiry) ResultString = TokenString();
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
            return string.Join(" ", Tokens);
        }
    }
}
