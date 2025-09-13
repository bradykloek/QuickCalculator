using System.Text;

namespace QuickCalculator.Errors
{
    internal class ErrorController
    {

        public static List<EvaluationError> Errors { get; private set; }  = new List<EvaluationError>();

        /// <summary>
        /// AddError handles any invalid input. It either throws an exception or stores the exception in Errors
        /// </summary>
        /// <param name="message"></param> Error Message
        /// <param name="charIndex"></param> Index of the input string that caused the error
        /// <exception cref="EvaluationError"></exception>
        public static void AddError(string message, int start, int end, ErrorSource source)
        {
            Errors.Add(new EvaluationError(message, start, end, source));
        }

        public static void ClearErrors()
        {
            Errors.Clear();
        }

        public static int Count()
        {
            return Errors.Count;
        }

        public static string ErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Evaluator encountered " + Errors.Count + " errors:\n");
            for (int i = 0; i < Errors.Count; i++)
            {
                sb.Append(" " + (i + 1) + ":   " + Errors[i].ToString() + "\n");
            }
            return sb.ToString();
        }
    }
}
