using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QuickCalculator
{
    public partial class InputWindow : Form
    {
        Color PAREN1 = Color.FromArgb(0, 200, 255);         // Blue
        Color PAREN2 = Color.FromArgb(255, 180, 0);         // Orange
        Color PAREN3 = Color.FromArgb(240, 30, 255);        // Purple
        Color NUMBER = Color.FromArgb(120, 240, 255);       // Turquoise
        Color OPERATOR = Color.FromArgb(160, 255, 150);     // Green
        Color SYMBOL = Color.FromArgb(180, 180, 255);       // Purple

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
                System.Windows.Forms.MessageBox.Show(string.Join(" ", (object[])tokenCollection.GetTokens()));
            }
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            // Don't throw exceptions because the user is currently typing
            TokenCollection tokenCollection = Tokenizer.Tokenize(inputTextBox.Text, false);
            bool[] errorIndices = tokenCollection.GetErrors();

            colorErrors(tokenCollection);
            colorTokens(tokenCollection);
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

        private void colorErrors(TokenCollection tokenCollection)
        {
            bool[] errorIndices = tokenCollection.GetErrors();
            for (int i = 0; i < errorIndices.Length; i++)
            {
                if (errorIndices[i]) // If index i caused an error
                {
                    colorText(i, 1, Color.Red, FontStyle.Underline);
                    Debug.WriteLine(i.ToString());
                }
            }
        }

        private void colorTokens(TokenCollection tokenCollection)
        {
            Token[] tokens = tokenCollection.GetTokens();
            Color[] parenColors = { PAREN1, PAREN2, PAREN3};
            for (int i = 0; i < tokenCollection.GetTokenCount(); i++)
            {
                int tokenStart = tokens[i].GetStart();
                int tokenLength = tokens[i].GetEnd() - tokenStart ; // Exclusive on right, so -1
                switch (tokens[i].GetCategory())
                {
                    case '(':
                    case ')':
                        int level = ((ParenToken)tokens[i]).GetLevel();
                        colorText(tokenStart, 1, parenColors[level % 3], FontStyle.Regular);
                        break;
                    case 'n':
                        colorText(tokenStart, tokenLength, NUMBER, FontStyle.Regular);
                        break;
                    case 'o':
                        colorText(tokenStart, 1, OPERATOR, FontStyle.Regular);
                        break;
                    case 's':
                        colorText(tokenStart, tokenLength, SYMBOL, FontStyle.Regular);
                        break;
                }
            }
        }

        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
