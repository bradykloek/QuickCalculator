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
        private List<EvaluationException> exceptions;       // If we don't throw exceptions, they will be stored here
        private bool throwExceptions;
        private double result;
        private Tokenizer tokenizer;
        private Parser parser;
        private string assignVariable = "";

        public Evaluator(string input, bool throwExceptions)
        {
            exceptions = new List<EvaluationException>(input.Length / 2);
            this.throwExceptions = throwExceptions;
            tokenizer = new Tokenizer(input, this);
            if (exceptions.Count() == 0)
            {
                parser = new Parser(tokenizer.GetTokens(), this);
                result = parser.GetResult();
            }
        }

        public double GetResult()
        {
            return result;
        }

        public bool GetThrowExceptions()
        {
            return throwExceptions;
        }

        public int GetExceptionCount()
        {
            return exceptions.Count();
        }

        public Tokenizer GetTokenizer()
        {
            return tokenizer;
        }

        public Parser GetParser()
        {
            return parser;
        }

        public List<EvaluationException> GetExceptions()
        {
            return exceptions;
        }

        public string ToString()
        {
            return result.ToString();
        }

        public void SetAssignVariable(string variableName)
        {
            Debug.WriteLine("ASSIGN!");
            assignVariable = variableName;
        }

        public string GetAssignVariable()
        {
            return assignVariable;
        }

        /// <summary>
        /// AddError handles any invalid input. It either throws an exception or stores the exception in exceptions
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index of the input string that caused the error
        /// <exception cref="EvaluationException"></exception>
        public void AddException(string message, int start, int end)
        {
            EvaluationException exception = new EvaluationException(message, start, end);

            if (throwExceptions)
            {
                throw exception;
            }
            else
            {
                exceptions.Add(exception);
            }
        }


        public string ErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Evaluator encountered " + exceptions.Count() + " errors:\n");
            for (int i = 0; i < exceptions.Count(); i++)
            {
                sb.Append(" " + (i + 1) + ":   " + exceptions[i].ToString() + "\n");
            }
            return sb.ToString();
        }

    }
}
