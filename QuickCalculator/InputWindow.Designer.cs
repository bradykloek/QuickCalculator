namespace QuickCalculator
{
    partial class InputWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            inputTextBox = new RichTextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // inputTextBox
            // 
            inputTextBox.BackColor = Color.FromArgb(64, 64, 64);
            inputTextBox.Font = new Font("Consolas", 12F);
            inputTextBox.ForeColor = Color.White;
            inputTextBox.Location = new Point(22, 26);
            inputTextBox.Margin = new Padding(6);
            inputTextBox.Name = "inputTextBox";
            inputTextBox.Size = new Size(1055, 55);
            inputTextBox.TabIndex = 1;
            inputTextBox.Text = "";
            inputTextBox.TextChanged += inputTextBox_TextChanged;
            inputTextBox.KeyDown += inputTextBox_KeyDown;
            // 
            // label1
            // 
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 4;
            // 
            // InputWindow
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DarkGray;
            ClientSize = new Size(1105, 106);
            Controls.Add(label1);
            Controls.Add(inputTextBox);
            Margin = new Padding(6);
            Name = "InputWindow";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox inputTextBox;
        private Label label1;
    }
}
