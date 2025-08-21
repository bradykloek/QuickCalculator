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
        private static List<int> tempIndices = new List<int>();


        public static void AddInput(string input)
        {
            RemoveTemporaries();
            inputs.Add(input);
            currentIndex = inputs.Count - 1;
        }

        private static void InsertTemporary(int change, string input)
        {
            // If the user is going toward more recent history, we want to insert the temporary after current index so we'll move it forward one
            if (change == 1) currentIndex += 1;

            inputs.Insert(currentIndex, input);
            tempIndices.Add(currentIndex);

            for (int i = 0; i < tempIndices.Count; i++)
            {
                if (tempIndices[i] > currentIndex) tempIndices[i]++;
            }
        }

        private static void RemoveTemporaries()
        {   // There will typically be very few elements in tempIndices, so this O(n^2) operation is negligible.
            while (tempIndices.Count > 0)
            {
                int maxIndex = 0;
                for(int i = 1; i < tempIndices.Count; i++)
                {
                    if (tempIndices[i] >= tempIndices[maxIndex])
                    {
                        maxIndex = i;
                    }
                }
                inputs.RemoveAt(tempIndices[maxIndex]);
                tempIndices.RemoveAt(maxIndex);
            }
        }

        public static string RetrieveInput(int change, string input)
        {
            if (inputs.Count == 0) return "";



            int newIndex = currentIndex + change;
            if (0 <= newIndex && newIndex < inputs.Count)
            {
                if (!input.Equals(inputs[currentIndex]))
                {
                    InsertTemporary(change, input);
                }

                currentIndex = newIndex;
                return inputs[currentIndex];
            }

            if (tempIndices.Contains(currentIndex))
            {

            }

            return inputs[currentIndex];
        }

        public static void Clear()
        {
            inputs = new List<string>();
            tempIndices = new List<int>();
            currentIndex = 0;
        }

        public static string HistoryString()
        {
            return currentIndex + "..." + (inputs.Count - currentIndex - 1);
        }
    }
}
