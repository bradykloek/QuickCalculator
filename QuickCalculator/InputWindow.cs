using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

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

                Tokenizer tokenizer = new Tokenizer(inputTextBox.Text, false);
                if(tokenizer.GetExceptions().Count() == 0)
                {
                    System.Windows.Forms.MessageBox.Show(string.Join(" ", (object[])tokenizer.GetTokens()));

                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(buildErrorMessage(tokenizer));

                }

            }
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            Tokenizer tokenizer = new Tokenizer(inputTextBox.Text, false);

            colorTokens(tokenizer);
            colorErrors(tokenizer);
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
        private void colorErrors(Tokenizer tokenizer)
        {
            List<TokenizerException> tokenizerExceptions = tokenizer.GetExceptions();
            for (int i = 0; i < tokenizerExceptions.Count(); i++)
            {
                colorText(tokenizerExceptions[i].GetCharIndex(), 1, Color.Red, FontStyle.Underline);
            }
        }

        /// <summary>
        /// Colors characters in the input text box to colors that indicate their token category
        /// </summary>
        /// <param name="tokenizer"></param> Collection of tokens that is parsed
        private void colorTokens(Tokenizer tokenizer)
        {
            Token[] tokens = tokenizer.GetTokens();
            Color[] parenColors = { PAREN1, PAREN2, PAREN3};
            for (int i = 0; i < tokenizer.GetTokenCount(); i++)
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
        
        private string buildErrorMessage(Tokenizer tokenizer)
        {
            StringBuilder sb = new StringBuilder();
            List<TokenizerException> tokenizerExceptions = tokenizer.GetExceptions();
            sb.Append("Tokenizer encountered " + tokenizerExceptions.Count() + " errors:\n");
            for(int i = 0; i < tokenizerExceptions.Count(); i++)
            {
                sb.Append(" " + (i+1) + ":   " + tokenizerExceptions[i].ToString() + "\n");
            }
            return sb.ToString();
        }
        
        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
