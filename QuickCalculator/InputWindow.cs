using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QuickCalculator
{
    public partial class InputWindow : Form
    {
        Color PAREN1 = Color.FromArgb(0, 200, 255);         // Blue
        Color PAREN2 = Color.FromArgb(255, 180, 0);         // Orange
        Color PAREN3 = Color.FromArgb(240, 30, 255);        // Purple
        Color NUMBER = Color.FromArgb(120, 240, 255);       // Turquoise
        Color OPERATOR = Color.FromArgb(160, 255, 150);     // Light Green
        Color VARIABLE = Color.FromArgb(180, 180, 255);     // Light Purple
        Color ASSIGNMENT = Color.FromArgb(255, 255, 70);    // Yellow
        Color FUNCTION = Color.FromArgb(255, 150, 255);     // Pink

        public InputWindow()
        {
            InitializeComponent();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // User's input should always be the default styling
            inputTextBox.SelectionColor = Color.White;
            inputTextBox.SelectionFont = new Font(inputTextBox.SelectionFont, FontStyle.Regular);

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                Evaluator evaluator = new Evaluator(inputTextBox.Text, false);

                if (evaluator.GetExceptionCount() == 0)
                {

                    System.Windows.Forms.MessageBox.Show(evaluator.ToString());
                    if (evaluator.GetAssignVariable() != "")
                    {   // If parser encountered no errors and a variable is to be assigned
                        Symbols.variables[evaluator.GetAssignVariable()] = evaluator.GetResult();
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(evaluator.ErrorMessage());
                }

            }
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            Evaluator evaluator = new Evaluator(inputTextBox.Text, false);

            colorTokens(evaluator);
            colorErrors(evaluator);
        }

        /// <summary>
        /// Colors select substrings of the input box text.
        /// </summary>
        /// <param name="start"></param> Start index that should be colored
        /// <param name="length"></param> Number of characters that should be colored
        /// <param name="color"></param> Color that the text should be changed to
        /// <param name="style"></param> Font Style that should be applied
        private void colorText(int start, int length, Color color, FontStyle style)
        {   // Save user's selection
            int selectionStart = inputTextBox.SelectionStart;
            int selectionLength = inputTextBox.SelectionLength;

            inputTextBox.Select(start, length);
            inputTextBox.SelectionColor = color;
            inputTextBox.SelectionFont = new Font(inputTextBox.SelectionFont, style);

            inputTextBox.SelectionStart = selectionStart;
            inputTextBox.SelectionLength = selectionLength;
        }

        /// <summary>
        /// Colors all characters that caused Tokenizer Errors to red with an underline
        /// </summary>
        /// <param name="tokenizer"></param> Collection of tokens that is parsed
        private void colorErrors(Evaluator evaluator)
        {
            List<EvaluationException> tokenizerExceptions = evaluator.GetExceptions();
            for (int i = 0; i < tokenizerExceptions.Count(); i++)
            {
                int start = tokenizerExceptions[i].GetStart();
                int end = tokenizerExceptions[i].GetEnd();
                colorText(start, end, Color.Red, FontStyle.Underline);
            }
        }

        /// <summary>
        /// Colors characters in the input text box to colors that indicate their token category
        /// </summary>
        /// <param name="tokenizer"></param> Collection of tokens that is parsed
        private void colorTokens(Evaluator evaluator)
        {
            // The only text that isn't a part of a token is for assignments, since the assignment operater and assignee variable are removed
            colorText(0, inputTextBox.Text.Length, ASSIGNMENT, FontStyle.Regular);
            List<Token> tokens = evaluator.GetTokenizer().GetTokens();
            Color[] parenColors = { PAREN1, PAREN2, PAREN3};
            for (int i = 0; i < tokens.Count(); i++)
            {
                int tokenStart = tokens[i].GetStart();
                int tokenLength = tokens[i].GetEnd() - tokenStart ; // Exclusive on right, so -1
                switch (tokens[i].GetCategory())
                {
                    case '(':
                    case ')':
                        int level = ((LevelToken)tokens[i]).GetLevel();
                        colorText(tokenStart, 1, parenColors[level % 3], FontStyle.Regular);
                        break;
                    case 'n':
                        colorText(tokenStart, tokenLength, NUMBER, FontStyle.Regular);
                        break;
                    case 'o':
                        colorText(tokenStart, tokenLength, OPERATOR, FontStyle.Regular);
                        break;
                    case 'v':
                        colorText(tokenStart, tokenLength, VARIABLE, FontStyle.Regular);
                        break;
                    case 'f':
                    case '[':
                    case ']':
                        colorText(tokenStart, tokenLength, FUNCTION, FontStyle.Regular);
                        break;
                }
            }
        }
        
        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
