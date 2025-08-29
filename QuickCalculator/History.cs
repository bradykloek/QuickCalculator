using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCalculator
{
    internal class History
    {
        private static List<string> entries = new List<string>();
        private static int index;


        public static void AddEntry(string entry)
        {
            if (entry.Length > 0 &&     // We don't want to add any empty entries to history
                (entries.Count == 0 || !entry.Equals(entries[entries.Count - 1])))  
                // We don't want to add an entry that is the same as the most recent entry
            {   
                entries.Add(entry);
                if (entries.Count > 0 && entries[index] == null)   
                // If the user has deleted the current history entry, we should complete this deletion now
                    entries.RemoveAt(index);
                index = entries.Count - 1;
            }
        }

        /// <summary>
        /// Attempts to retrieve an adjacent history entry
        /// </summary>
        /// <param name="change"></param> Either 1 or -1, determining which direction the user is going in history
        /// <param name="currentInput"></param> The text that is currently in the input box when they initiated this retrieval attempt
        /// <returns></returns>
        public static string RetrieveEntry(int change, string currentInput)
        {
            if(entries.Count == 0 || entries.Count == 1 && entries[0] == null)
                // If there aren't any entries that aren't marked for deletion, do nothing by returning the same input the user already had
                return currentInput;

            if (currentInput.Equals("") && entries[index] != null)
                // If the user had a blank input, retrieve the most recent entry
                return entries[index];

            if(entries.Count == 1)
            {   /* Handles an edge case where there is only one entry in the list, as this would
                 * normally go out of bounds when the adjacent index is accessed. */
                if (!currentInput.Equals(entries[index]))
                {
                    entries.Add(currentInput);
                    return entries[index];
                }
                else return currentInput;
            }

            if (0 > index + change || index + change >= entries.Count)
                // If the user is attempting to access an out of bounds entry, do nothing
                return currentInput;

            if (entries[index] == null)
            {   // The current entry is marked for deletion
                if (currentInput.Equals(""))
                {   // If the user's input is empty, remove the entry
                    entries.RemoveAt(index);
                    if (change == -1) index--;
                    return entries[index];
                }
                else entries[index] = currentInput;
                // If the user's input is not empty, replace null with their input
            }

            if (!currentInput.Equals(entries[index]))
                // If the user's input is different from the current entry, insert it into the history
                entries.Insert(++index, currentInput);

            index += change;
            return entries[index];
        }

        /// <summary>
        /// When the user deletes an entry, we don't remove it from the list right away because we want to maintain the current position in
        /// the entries list. Instead, we mark it for deletion by setting it to null. We will lazily delete it later once the user moves to a new
        /// entry in history.
        /// </summary>
        public static void MarkDeletion()
        {
            entries[index] = null;
        }

        public static void Clear()
        {
            entries = new List<string>();
            index = 0;
        }

        public static string HistoryString()
        {
            return index + "..." + (entries.Count - index - 1);
        }
    }
}
