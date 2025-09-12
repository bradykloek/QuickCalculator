    using QuickCalculator.Evaluation;
    using QuickCalculator.Tokens;

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
            Color FUNC1 = Color.FromArgb(255, 150, 255);        // Pink
            Color FUNC2 = Color.FromArgb(150, 255, 200);        // Mint
            Color FUNC3 = Color.FromArgb(255, 190, 190);        // Faded Red
            Color PARAMETER = Color.FromArgb(255, 255, 160);    // Faded Yellow

            Color DEFAULT = Color.FromArgb(64, 64, 64);         // Dark Grey
            Color SUCCESS = Color.FromArgb(0, 80, 0);           // Green
            Color DELETE = Color.FromArgb(80, 0, 0);            // Red

            double roundPrecision = 0.00000001;

            bool TemporaryResult = false;


            public InputWindow()
            {
                InitializeComponent();
            }

            private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (TemporaryResult)
                {
                    inputTextBox.Text = "";
                    TemporaryResult = false;
                }

                switch (e.KeyCode)
                {

                    case Keys.Enter:
                        e.SuppressKeyPress = true;

                        if (inputTextBox.Text.Length == 0) break;

                        History.AddEntry(inputTextBox.Text);
                        Evaluator evaluator = new Evaluator(true, roundPrecision);
                        evaluator.Evaluate(inputTextBox.Text);

                        if (ExceptionController.Count() != 0)
                            System.Windows.Forms.MessageBox.Show(ExceptionController.ErrorMessage());
                        else
                        {
                            TemporaryResult = evaluator.TemporaryResult;
                            if (TemporaryResult)
                            {
                                inputTextBox.Text = evaluator.ResultString;
                                return;
                            }

                            UpdateTextBox(evaluator.ResultString);
                            History.AddEntry(evaluator.ResultString);
                            inputTextBox.BackColor = SUCCESS;
                        }
                        break;
                    case Keys.Up:
                        e.SuppressKeyPress = true;

                        UpdateTextBox(History.RetrieveEntry(-1, inputTextBox.Text));
                        break;
                    case Keys.Down:
                        e.SuppressKeyPress = true;

                        UpdateTextBox(History.RetrieveEntry(1, inputTextBox.Text));
                        break;

                    case Keys.Back:
                        if (e.Control && e.Alt)
                        {
                            UpdateTextBox("");
                            History.MarkDeletion();
                            inputTextBox.BackColor = DELETE;

                        }
                        break;
                }

            }

            private void inputTextBox_TextChanged(object sender, EventArgs e)
            {
                if (TemporaryResult) return;
                if (inputTextBox.Text.Length == 0) return;
                Evaluator evaluator = new Evaluator(false, 0);
                evaluator.Evaluate(inputTextBox.Text);
                inputTextBox.BackColor = DEFAULT;
                colorTokens(evaluator);
                colorErrors(evaluator);
            }

            private void UpdateTextBox(string content)
            {
                inputTextBox.Text = content;
                inputTextBox.Select(content.Length, 0);
            }

            /// <summary>
            /// Colors select substrings of the input box text.
            /// </summary>
            /// <param name="start"></param> Start index that should be colored
            /// <param name="length"></param> Number of characters that should be colored
            /// <param name="color"></param> Color that the text should be changed to
            /// <param name="style"></param> Font Style that should be applied
            private void colorText(int start, int length, Color color, FontStyle style = FontStyle.Regular)
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
            /// <param name="tokenizer"></param> Collection of Tokens that is parsed
            private void colorErrors(Evaluator evaluator)
            {
                List<EvaluationException> Exceptions = ExceptionController.Exceptions;
                for (int i = 0; i < Exceptions.Count; i++)
                {
                    int start = Exceptions[i].StartIndex;
                    int end = Exceptions[i].EndIndex;
                    colorText(start, end, Color.Red, FontStyle.Underline);
                }
            }

            /// <summary>
            /// Colors characters in the input text box to colors that indicate their token category
            /// </summary>
            /// <param name="tokenizer"></param> Collection of Tokens that is parsed
            private void colorTokens(Evaluator evaluator)
            {
                // The only text that isn't a part of a token is for assignments, since the assignment operater and assignee variable are removed
                colorText(0, inputTextBox.Text.Length, ASSIGNMENT);
                List<Token> Tokens = evaluator.Tokens;
                Color[] parenColors = { PAREN1, PAREN2, PAREN3 };
                Color[] funcColors = { FUNC1, FUNC2, FUNC3 };

                int level;
                for (int i = 0; i < Tokens.Count; i++)
                {
                    int tokenStart = Tokens[i].StartIndex;
                    int tokenLength = Tokens[i].EndIndex - tokenStart;
                    switch (Tokens[i].category)
                    {
                        case TokenCategory.OpenParen:
                        case TokenCategory.CloseParen:
                            level = ((LevelToken)Tokens[i]).Level;
                            colorText(tokenStart, 1, parenColors[level % 3]);
                            break;
                        case TokenCategory.Number:
                            colorText(tokenStart, tokenLength, NUMBER);
                            break;
                        case TokenCategory.Operator:
                            colorText(tokenStart, tokenLength, OPERATOR);
                            break;
                        case TokenCategory.Variable:
                            colorText(tokenStart, tokenLength, VARIABLE);
                            break;
                        case TokenCategory.Function:
                        case TokenCategory.OpenBracket:
                        case TokenCategory.CloseBracket:
                        case TokenCategory.Comma:
                            // All of these Tokens are only found in function calls
                            level = ((LevelToken)Tokens[i]).Level;
                            colorText(tokenStart, tokenLength, funcColors[level % 3]);
                            break;
                        case TokenCategory.Parameter:
                            colorText(tokenStart, tokenLength, PARAMETER);
                            break;
                    }
                }
            }
        }
    }
