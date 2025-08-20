using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class History
    {
        private static List<string> inputs = new List<string>();
        private static int currentIndex = 0;

        public static void AddInput(string input)
        {
            inputs.Add(input);
            currentIndex = inputs.Count() - 1;
        }

        public static string RetrieveInput(int change)
        {
            if (inputs.Count() == 0) return "";

            int newIndex = currentIndex + change;
            if (0 <= newIndex && newIndex < inputs.Count())
            {
                currentIndex = newIndex;
                return inputs[currentIndex];
            }
            return inputs[currentIndex];
        }

        public static string RetrieveInput(int change, string input)
        {
            if (inputs.Count() == 0) return "";

            if (!input.Equals(inputs[currentIndex]))
            {
                inputs.Insert(currentIndex + change, input);
                currentIndex = currentIndex + change;
            }

            int newIndex = currentIndex + change;
            if (0 <= newIndex && newIndex < inputs.Count())
            {
                currentIndex = newIndex;
                return inputs[currentIndex];
            }
            return inputs[currentIndex];
        }

        public static string HistoryString()
        {
            return currentIndex + "..." + (inputs.Count() - currentIndex - 1);
        }
    }
}
