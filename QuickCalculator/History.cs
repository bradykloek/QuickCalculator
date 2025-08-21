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
        private static int currentIndex;

        public static void AddInput(string input)
        {
            if(input.Length > 0)
            {   // We don't want to add any empty inputs to history
                if (inputs.Count > 0 && inputs[currentIndex] == null)   // If the user has deleted the current history entry
                    CompleteDeletion();
                inputs.Add(input);
                currentIndex = inputs.Count - 1;
            }
        }

        public static string RetrieveInput(int change, string currentInput)
        {
            if(inputs.Count() == 0 || currentIndex + change < 0 || currentIndex + change >= inputs.Count())
                // If there is no history yet or the entry would be out of bounds, just return the same input
                return currentInput;   


            if (inputs[currentIndex] == null)   // If the user has deleted the current history entry
                CompleteDeletion(change);

            if(!currentInput.Equals(""))
            {
                if (!currentInput.Equals(inputs[currentIndex]))
                    SaveCurrentInput(change, currentInput);
                currentIndex += change;
            }
            return inputs[currentIndex];

        }


        private static void SaveCurrentInput(int change, string currentInput)
        {
            if (currentIndex == inputs.Count - 1)
            {   // If the user is at the present point and is going into history, we want to save their input at the end of the list
                AddInput(currentInput);
            }
            else
            {
                // If the user is going toward more recent history, we want to insert the temporary after current index
                if (change == 1) { currentIndex += 1; }
                // Otherwise they are going toward past history, and we can insert at current index because that will shift everything else forward

                inputs.Insert(currentIndex, currentInput);
            }
        }

        public static void MarkDeletion()
        {
            inputs[currentIndex] = null;
            /* Set this entry to null which will mark it for deletion. This cell of the list won't actually be removed until the user moves
             * through history or a new entry is added to history, since we still want to maintain the current index. */
        }

        private static void CompleteDeletion(int change = 0)
        {
            inputs.RemoveAt(currentIndex);
            if (change == -1) currentIndex--;
            /* If the user went backward in memory we'll need to move back. If they went forward (change == 1) the entry removal will have shifted the list
             * so currentIndex will already be on the correct entry. */

        }
        public static void Clear()
        {
            inputs = new List<string>();
            currentIndex = 0;
        }

        public static string HistoryString()
        {
            return currentIndex + "..." + (inputs.Count - currentIndex - 1);
        }
    }
}
