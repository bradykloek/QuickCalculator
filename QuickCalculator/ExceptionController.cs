using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class ExceptionController
    {

        public static List<EvaluationException> Exceptions { get; private set; } 
            = new List<EvaluationException>();

        /// <summary>
        /// AddException handles any invalid input. It either throws an exception or stores the exception in Exceptions
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index of the input string that caused the error
        /// <exception cref="EvaluationException"></exception>
        public static void AddException(string message, int start, int end, char Source)
        {
            Exceptions.Add(new EvaluationException(message, start, end, Source));
        }

        public static void ClearExceptions()
        {
            Exceptions.Clear();
        }

        public static int Count()
        {
            return Exceptions.Count;
        }

        public static string ErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Evaluator encountered " + Exceptions.Count + " errors:\n");
            for (int i = 0; i < Exceptions.Count; i++)
            {
                sb.Append(" " + (i + 1) + ":   " + Exceptions[i].ToString() + "\n");
            }
            return sb.ToString();
        }
    }
}
