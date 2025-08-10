using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class ExceptionController
    {

        private static List<EvaluationException> exceptions = new List<EvaluationException>();

        /// <summary>
        /// AddException handles any invalid input. It either throws an exception or stores the exception in exceptions
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index of the input string that caused the error
        /// <exception cref="EvaluationException"></exception>
        public static void AddException(string message, int start, int end, char source)
        {
            exceptions.Add(new EvaluationException(message, start, end, source));
        }

        public static void ClearExceptions()
        {
            exceptions.Clear();
        }

        public static int Count()
        {
            return exceptions.Count;
        }

        public static List<EvaluationException> GetExceptions()
        {
            return exceptions;
        }

        public static string ErrorMessage()
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
