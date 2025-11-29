namespace PgGenFldCode
{
    partial class GenFldFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            genCodeButton = new Button();
            executiveTextLabel = new Label();
            sqlTextBox = new TextBox();
            splitContainer1 = new SplitContainer();
            ExecTypeLabel = new Label();
            execTypeComboBox = new ComboBox();
            LangLabel = new Label();
            optionsLabel = new Label();
            langComboBox = new ComboBox();
            progressBar = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // genCodeButton
            // 
            genCodeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            genCodeButton.Location = new Point(3, 549);
            genCodeButton.Name = "genCodeButton";
            genCodeButton.Size = new Size(107, 48);
            genCodeButton.TabIndex = 2;
            genCodeButton.Text = "GenCode";
            genCodeButton.UseVisualStyleBackColor = true;
            // 
            // executiveTextLabel
            // 
            executiveTextLabel.AutoSize = true;
            executiveTextLabel.Location = new Point(3, 47);
            executiveTextLabel.Name = "executiveTextLabel";
            executiveTextLabel.Size = new Size(105, 20);
            executiveTextLabel.TabIndex = 1;
            executiveTextLabel.Text = "Executive Text:";
            // 
            // sqlTextBox
            // 
            sqlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sqlTextBox.Location = new Point(3, 70);
            sqlTextBox.Multiline = true;
            sqlTextBox.Name = "sqlTextBox";
            sqlTextBox.Size = new Size(617, 473);
            sqlTextBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(2, 1);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(ExecTypeLabel);
            splitContainer1.Panel1.Controls.Add(execTypeComboBox);
            splitContainer1.Panel1.Controls.Add(LangLabel);
            splitContainer1.Panel1.Controls.Add(optionsLabel);
            splitContainer1.Panel1.Controls.Add(langComboBox);
            splitContainer1.Panel1.Controls.Add(progressBar);
            splitContainer1.Panel1.Controls.Add(genCodeButton);
            splitContainer1.Panel1.Controls.Add(sqlTextBox);
            splitContainer1.Panel1.Controls.Add(executiveTextLabel);
            splitContainer1.Size = new Size(1234, 600);
            splitContainer1.SplitterDistance = 623;
            splitContainer1.TabIndex = 1;
            // 
            // ExecTypeLabel
            // 
            ExecTypeLabel.AutoSize = true;
            ExecTypeLabel.Location = new Point(370, 14);
            ExecTypeLabel.Name = "ExecTypeLabel";
            ExecTypeLabel.Size = new Size(73, 20);
            ExecTypeLabel.TabIndex = 8;
            ExecTypeLabel.Text = "ExecType:";
            // 
            // execTypeComboBox
            // 
            execTypeComboBox.FormattingEnabled = true;
            execTypeComboBox.Location = new Point(449, 11);
            execTypeComboBox.Name = "execTypeComboBox";
            execTypeComboBox.Size = new Size(171, 28);
            execTypeComboBox.TabIndex = 7;
            // 
            // LangLabel
            // 
            LangLabel.AutoSize = true;
            LangLabel.Location = new Point(116, 14);
            LangLabel.Name = "LangLabel";
            LangLabel.Size = new Size(44, 20);
            LangLabel.TabIndex = 6;
            LangLabel.Text = "Lang:";
            // 
            // optionsLabel
            // 
            optionsLabel.AutoSize = true;
            optionsLabel.Location = new Point(5, 14);
            optionsLabel.Name = "optionsLabel";
            optionsLabel.Size = new Size(64, 20);
            optionsLabel.TabIndex = 5;
            optionsLabel.Text = "Options:";
            // 
            // langComboBox
            // 
            langComboBox.FormattingEnabled = true;
            langComboBox.Location = new Point(166, 11);
            langComboBox.Name = "langComboBox";
            langComboBox.Size = new Size(171, 28);
            langComboBox.TabIndex = 4;
            // 
            // progressBar
            // 
            progressBar.ForeColor = Color.FromArgb(255, 192, 255);
            progressBar.Location = new Point(116, 549);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(504, 48);
            progressBar.TabIndex = 3;
            progressBar.Value = 10;
            // 
            // GenFldFrm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1238, 604);
            Controls.Add(splitContainer1);
            Name = "GenFldFrm";
            Text = "GenFldFrm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TextBox sqlTextBox;
        private Label executiveTextLabel;
        private Button genCodeButton;
        private SplitContainer splitContainer1;
        private ProgressBar progressBar;
        private ComboBox langComboBox;
        private Label LangLabel;
        private Label optionsLabel;
        private Label ExecTypeLabel;
        private ComboBox execTypeComboBox;
    }
}