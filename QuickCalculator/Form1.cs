namespace QuickCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LinkedList tokens = Tokenizer.Tokenize(inputTextBox.Text);
                System.Windows.Forms.MessageBox.Show(tokens.ToString());
            }
        }
    }
}
