using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QuickCalculator
{
    public partial class InputWindow : Form
    {
        Color BLUE = Color.FromArgb(0, 200, 255);
        Color ORANGE = Color.FromArgb(255, 180, 0);
        Color PURPLE = Color.FromArgb(240, 30, 255);
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

                // We want to throw any exceptions after the user hits Enter
                TokenCollection tokenCollection = Tokenizer.Tokenize(inputTextBox.Text, true);
                System.Windows.Forms.MessageBox.Show(string.Join(" ", (object[])tokenCollection.getTokens()));
            }
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {


            // Don't throw exceptions because the user is currently typing
            TokenCollection tokenCollection = Tokenizer.Tokenize(inputTextBox.Text, false);
            bool[] errorIndices = tokenCollection.getErrors();

            for (int i = 0; i < errorIndices.Length; i++)
            {
                if (errorIndices[i]) // If index i caused an error
                {
                    colorText(i, 1, Color.Red, FontStyle.Underline);
                    Debug.WriteLine(i.ToString());
                }
            }

            colorParens(tokenCollection);
        }

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

        private void colorParens(TokenCollection tokenCollection)
        {
            // Blue, Orange, Purple
            Color[] parenColors = { BLUE, ORANGE, PURPLE};
            Token[] tokens = tokenCollection.getTokens();
            string text = inputTextBox.Text;
            int tokenIdx = 0;
            for (int charIdx = 0; charIdx < text.Length; charIdx++)
            {
                if (text[charIdx] == '(' || text[charIdx] == ')')
                {
                    while (tokens[tokenIdx].GetCategory() != text[charIdx])
                    {
                        tokenIdx++;
                        if (tokenIdx >= tokens.Length)
                        {
                            throw new IndexOutOfRangeException("Token and Text mismatch.");
                        }
                    }
                    int level = ((ParenToken)tokens[tokenIdx]).GetLevel();
                    tokenIdx++;
                    Debug.WriteLine("" + charIdx + " : " + level);
                    colorText(charIdx, 1, parenColors[level % 3], FontStyle.Regular);

                }
            }
        }

        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
