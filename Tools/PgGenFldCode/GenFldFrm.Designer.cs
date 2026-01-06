namespace Tools.PgGenFldCode
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
            tblMasterSrvNameTextBox = new TextBox();
            TblMasterSrvNameLabel = new Label();
            tblMasterDescTextBox = new TextBox();
            tblMasterDescLabel = new Label();
            tblMasterCodeTextBox = new TextBox();
            tblMasterCodeLabel = new Label();
            outputGenSqlScriptCheckBox = new CheckBox();
            outputGenCSharpFrameCheckBox = new CheckBox();
            autoCommitCheckBox = new CheckBox();
            useLogCheckBox = new CheckBox();
            ExecTypeLabel = new Label();
            execTypeComboBox = new ComboBox();
            LangLabel = new Label();
            optionsLabel = new Label();
            langComboBox = new ComboBox();
            progressBar = new ProgressBar();
            resultTabControl = new TabControl();
            CSharpReadModelTabPage = new TabPage();
            cSharpReadModelTextBox = new TextBox();
            DomainTabPage = new TabPage();
            domainTextBox = new TextBox();
            TblFieldTabPage = new TabPage();
            jsonViewerTabPage = new TabPage();
            JsonViewerTextBox = new TextBox();
            panel1 = new Panel();
            hostCommitTextBox = new TextBox();
            databaseResourceTextBox = new TextBox();
            hostCommitLabel = new Label();
            databaseResourceLabel = new Label();
            tblFieldTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            resultTabControl.SuspendLayout();
            CSharpReadModelTabPage.SuspendLayout();
            DomainTabPage.SuspendLayout();
            TblFieldTabPage.SuspendLayout();
            jsonViewerTabPage.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // genCodeButton
            // 
            genCodeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            genCodeButton.Location = new Point(3, 583);
            genCodeButton.Name = "genCodeButton";
            genCodeButton.Size = new Size(107, 48);
            genCodeButton.TabIndex = 2;
            genCodeButton.Text = "GenCode";
            genCodeButton.UseVisualStyleBackColor = true;
            genCodeButton.Click += GenCodeButtonClick;
            // 
            // executiveTextLabel
            // 
            executiveTextLabel.AutoSize = true;
            executiveTextLabel.Location = new Point(9, 183);
            executiveTextLabel.Name = "executiveTextLabel";
            executiveTextLabel.Size = new Size(105, 20);
            executiveTextLabel.TabIndex = 1;
            executiveTextLabel.Text = "Executive Text:";
            // 
            // sqlTextBox
            // 
            sqlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sqlTextBox.Location = new Point(3, 206);
            sqlTextBox.Multiline = true;
            sqlTextBox.Name = "sqlTextBox";
            sqlTextBox.Size = new Size(617, 371);
            sqlTextBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(2, 77);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tblMasterSrvNameTextBox);
            splitContainer1.Panel1.Controls.Add(TblMasterSrvNameLabel);
            splitContainer1.Panel1.Controls.Add(tblMasterDescTextBox);
            splitContainer1.Panel1.Controls.Add(tblMasterDescLabel);
            splitContainer1.Panel1.Controls.Add(tblMasterCodeTextBox);
            splitContainer1.Panel1.Controls.Add(tblMasterCodeLabel);
            splitContainer1.Panel1.Controls.Add(outputGenSqlScriptCheckBox);
            splitContainer1.Panel1.Controls.Add(outputGenCSharpFrameCheckBox);
            splitContainer1.Panel1.Controls.Add(autoCommitCheckBox);
            splitContainer1.Panel1.Controls.Add(useLogCheckBox);
            splitContainer1.Panel1.Controls.Add(ExecTypeLabel);
            splitContainer1.Panel1.Controls.Add(execTypeComboBox);
            splitContainer1.Panel1.Controls.Add(LangLabel);
            splitContainer1.Panel1.Controls.Add(optionsLabel);
            splitContainer1.Panel1.Controls.Add(langComboBox);
            splitContainer1.Panel1.Controls.Add(progressBar);
            splitContainer1.Panel1.Controls.Add(genCodeButton);
            splitContainer1.Panel1.Controls.Add(sqlTextBox);
            splitContainer1.Panel1.Controls.Add(executiveTextLabel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(resultTabControl);
            splitContainer1.Size = new Size(1234, 634);
            splitContainer1.SplitterDistance = 623;
            splitContainer1.TabIndex = 1;
            // 
            // tblMasterSrvNameTextBox
            // 
            tblMasterSrvNameTextBox.Location = new Point(155, 151);
            tblMasterSrvNameTextBox.Name = "tblMasterSrvNameTextBox";
            tblMasterSrvNameTextBox.Size = new Size(465, 27);
            tblMasterSrvNameTextBox.TabIndex = 15;
            // 
            // TblMasterSrvNameLabel
            // 
            TblMasterSrvNameLabel.AutoSize = true;
            TblMasterSrvNameLabel.Location = new Point(6, 154);
            TblMasterSrvNameLabel.Name = "TblMasterSrvNameLabel";
            TblMasterSrvNameLabel.Size = new Size(145, 20);
            TblMasterSrvNameLabel.TabIndex = 14;
            TblMasterSrvNameLabel.Text = "TblMaster.SrvName: ";
            // 
            // tblMasterDescTextBox
            // 
            tblMasterDescTextBox.Location = new Point(155, 118);
            tblMasterDescTextBox.Name = "tblMasterDescTextBox";
            tblMasterDescTextBox.Size = new Size(465, 27);
            tblMasterDescTextBox.TabIndex = 13;
            // 
            // tblMasterDescLabel
            // 
            tblMasterDescLabel.AutoSize = true;
            tblMasterDescLabel.Location = new Point(6, 121);
            tblMasterDescLabel.Name = "tblMasterDescLabel";
            tblMasterDescLabel.Size = new Size(117, 20);
            tblMasterDescLabel.TabIndex = 12;
            tblMasterDescLabel.Text = "TblMaster.Desc: ";
            // 
            // tblMasterCodeTextBox
            // 
            tblMasterCodeTextBox.Location = new Point(155, 85);
            tblMasterCodeTextBox.Name = "tblMasterCodeTextBox";
            tblMasterCodeTextBox.Size = new Size(465, 27);
            tblMasterCodeTextBox.TabIndex = 11;
            // 
            // tblMasterCodeLabel
            // 
            tblMasterCodeLabel.AutoSize = true;
            tblMasterCodeLabel.Location = new Point(6, 88);
            tblMasterCodeLabel.Name = "tblMasterCodeLabel";
            tblMasterCodeLabel.Size = new Size(120, 20);
            tblMasterCodeLabel.TabIndex = 10;
            tblMasterCodeLabel.Text = "TblMaster.Code: ";
            // 
            // outputGenSqlScriptCheckBox
            // 
            outputGenSqlScriptCheckBox.AutoSize = true;
            outputGenSqlScriptCheckBox.Enabled = false;
            outputGenSqlScriptCheckBox.Location = new Point(116, 45);
            outputGenSqlScriptCheckBox.Name = "outputGenSqlScriptCheckBox";
            outputGenSqlScriptCheckBox.Size = new Size(162, 24);
            outputGenSqlScriptCheckBox.TabIndex = 9;
            outputGenSqlScriptCheckBox.Text = "OutputGenSqlScript";
            outputGenSqlScriptCheckBox.UseVisualStyleBackColor = true;
            // 
            // outputGenCSharpFrameCheckBox
            // 
            outputGenCSharpFrameCheckBox.AutoSize = true;
            outputGenCSharpFrameCheckBox.Checked = true;
            outputGenCSharpFrameCheckBox.CheckState = CheckState.Checked;
            outputGenCSharpFrameCheckBox.Location = new Point(311, 45);
            outputGenCSharpFrameCheckBox.Name = "outputGenCSharpFrameCheckBox";
            outputGenCSharpFrameCheckBox.Size = new Size(162, 24);
            outputGenCSharpFrameCheckBox.TabIndex = 9;
            outputGenCSharpFrameCheckBox.Text = "OutputGenC#Frame";
            outputGenCSharpFrameCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoCommitCheckBox
            // 
            autoCommitCheckBox.AutoSize = true;
            autoCommitCheckBox.Location = new Point(504, 45);
            autoCommitCheckBox.Name = "autoCommitCheckBox";
            autoCommitCheckBox.Size = new Size(116, 24);
            autoCommitCheckBox.TabIndex = 9;
            autoCommitCheckBox.Text = "AutoCommit";
            autoCommitCheckBox.UseVisualStyleBackColor = true;
            // 
            // useLogCheckBox
            // 
            useLogCheckBox.AutoSize = true;
            useLogCheckBox.Location = new Point(10, 45);
            useLogCheckBox.Name = "useLogCheckBox";
            useLogCheckBox.Size = new Size(80, 24);
            useLogCheckBox.TabIndex = 9;
            useLogCheckBox.Text = "UseLog";
            useLogCheckBox.UseVisualStyleBackColor = true;
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
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.ForeColor = Color.FromArgb(255, 192, 255);
            progressBar.Location = new Point(116, 583);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(504, 48);
            progressBar.TabIndex = 3;
            // 
            // resultTabControl
            // 
            resultTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            resultTabControl.Controls.Add(CSharpReadModelTabPage);
            resultTabControl.Controls.Add(DomainTabPage);
            resultTabControl.Controls.Add(TblFieldTabPage);
            resultTabControl.Controls.Add(jsonViewerTabPage);
            resultTabControl.Location = new Point(3, 3);
            resultTabControl.Name = "resultTabControl";
            resultTabControl.SelectedIndex = 0;
            resultTabControl.Size = new Size(601, 628);
            resultTabControl.TabIndex = 0;
            // 
            // CSharpReadModelTabPage
            // 
            CSharpReadModelTabPage.Controls.Add(cSharpReadModelTextBox);
            CSharpReadModelTabPage.Location = new Point(4, 29);
            CSharpReadModelTabPage.Name = "CSharpReadModelTabPage";
            CSharpReadModelTabPage.Padding = new Padding(3);
            CSharpReadModelTabPage.Size = new Size(593, 595);
            CSharpReadModelTabPage.TabIndex = 1;
            CSharpReadModelTabPage.Text = "CSharpRModel";
            CSharpReadModelTabPage.UseVisualStyleBackColor = true;
            // 
            // cSharpReadModelTextBox
            // 
            cSharpReadModelTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cSharpReadModelTextBox.ImeMode = ImeMode.NoControl;
            cSharpReadModelTextBox.Location = new Point(3, 6);
            cSharpReadModelTextBox.Multiline = true;
            cSharpReadModelTextBox.Name = "cSharpReadModelTextBox";
            cSharpReadModelTextBox.Size = new Size(587, 583);
            cSharpReadModelTextBox.TabIndex = 2;
            // 
            // DomainTabPage
            // 
            DomainTabPage.Controls.Add(domainTextBox);
            DomainTabPage.Location = new Point(4, 29);
            DomainTabPage.Name = "DomainTabPage";
            DomainTabPage.Padding = new Padding(3);
            DomainTabPage.Size = new Size(593, 595);
            DomainTabPage.TabIndex = 2;
            DomainTabPage.Text = "Domain";
            DomainTabPage.UseVisualStyleBackColor = true;
            // 
            // domainTextBox
            // 
            domainTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            domainTextBox.ImeMode = ImeMode.NoControl;
            domainTextBox.Location = new Point(3, 6);
            domainTextBox.Multiline = true;
            domainTextBox.Name = "domainTextBox";
            domainTextBox.Size = new Size(587, 583);
            domainTextBox.TabIndex = 3;
            // 
            // TblFieldTabPage
            // 
            TblFieldTabPage.Controls.Add(tblFieldTextBox);
            TblFieldTabPage.Location = new Point(4, 29);
            TblFieldTabPage.Name = "TblFieldTabPage";
            TblFieldTabPage.Padding = new Padding(3);
            TblFieldTabPage.Size = new Size(593, 595);
            TblFieldTabPage.TabIndex = 3;
            TblFieldTabPage.Text = "TblField";
            TblFieldTabPage.UseVisualStyleBackColor = true;
            // 
            // jsonViewerTabPage
            // 
            jsonViewerTabPage.Controls.Add(JsonViewerTextBox);
            jsonViewerTabPage.Location = new Point(4, 29);
            jsonViewerTabPage.Name = "jsonViewerTabPage";
            jsonViewerTabPage.Padding = new Padding(3);
            jsonViewerTabPage.Size = new Size(593, 595);
            jsonViewerTabPage.TabIndex = 0;
            jsonViewerTabPage.Text = "JsonViewer";
            jsonViewerTabPage.UseVisualStyleBackColor = true;
            // 
            // JsonViewerTextBox
            // 
            JsonViewerTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            JsonViewerTextBox.Location = new Point(3, 6);
            JsonViewerTextBox.Multiline = true;
            JsonViewerTextBox.Name = "JsonViewerTextBox";
            JsonViewerTextBox.Size = new Size(587, 583);
            JsonViewerTextBox.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(hostCommitTextBox);
            panel1.Controls.Add(databaseResourceTextBox);
            panel1.Controls.Add(hostCommitLabel);
            panel1.Controls.Add(databaseResourceLabel);
            panel1.Location = new Point(2, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(1234, 77);
            panel1.TabIndex = 2;
            // 
            // hostCommitTextBox
            // 
            hostCommitTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hostCommitTextBox.Location = new Point(155, 43);
            hostCommitTextBox.Name = "hostCommitTextBox";
            hostCommitTextBox.Size = new Size(1069, 27);
            hostCommitTextBox.TabIndex = 3;
            // 
            // databaseResourceTextBox
            // 
            databaseResourceTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            databaseResourceTextBox.Location = new Point(155, 9);
            databaseResourceTextBox.Name = "databaseResourceTextBox";
            databaseResourceTextBox.Size = new Size(1069, 27);
            databaseResourceTextBox.TabIndex = 2;
            // 
            // hostCommitLabel
            // 
            hostCommitLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            hostCommitLabel.AutoSize = true;
            hostCommitLabel.Location = new Point(10, 46);
            hostCommitLabel.Name = "hostCommitLabel";
            hostCommitLabel.Size = new Size(104, 20);
            hostCommitLabel.TabIndex = 1;
            hostCommitLabel.Text = "Host Commit: ";
            // 
            // databaseResourceLabel
            // 
            databaseResourceLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            databaseResourceLabel.AutoSize = true;
            databaseResourceLabel.Location = new Point(10, 12);
            databaseResourceLabel.Name = "databaseResourceLabel";
            databaseResourceLabel.Size = new Size(143, 20);
            databaseResourceLabel.TabIndex = 0;
            databaseResourceLabel.Text = "Database Resource: ";
            // 
            // tblFieldTextBox
            // 
            tblFieldTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tblFieldTextBox.ImeMode = ImeMode.NoControl;
            tblFieldTextBox.Location = new Point(3, 6);
            tblFieldTextBox.Multiline = true;
            tblFieldTextBox.Name = "tblFieldTextBox";
            tblFieldTextBox.Size = new Size(587, 583);
            tblFieldTextBox.TabIndex = 3;
            // 
            // GenFldFrm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1238, 714);
            Controls.Add(panel1);
            Controls.Add(splitContainer1);
            Name = "GenFldFrm";
            Text = "GenFldFrm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            resultTabControl.ResumeLayout(false);
            CSharpReadModelTabPage.ResumeLayout(false);
            CSharpReadModelTabPage.PerformLayout();
            DomainTabPage.ResumeLayout(false);
            DomainTabPage.PerformLayout();
            TblFieldTabPage.ResumeLayout(false);
            TblFieldTabPage.PerformLayout();
            jsonViewerTabPage.ResumeLayout(false);
            jsonViewerTabPage.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
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
        private CheckBox useLogCheckBox;
        private CheckBox outputGenSqlScriptCheckBox;
        private CheckBox autoCommitCheckBox;
        private CheckBox outputGenCSharpFrameCheckBox;
        private Panel panel1;
        private TextBox hostCommitTextBox;
        private TextBox databaseResourceTextBox;
        private Label hostCommitLabel;
        private Label databaseResourceLabel;
        private Label tblMasterCodeLabel;
        private TextBox tblMasterCodeTextBox;
        private TextBox tblMasterDescTextBox;
        private Label tblMasterDescLabel;
        private TextBox tblMasterSrvNameTextBox;
        private Label TblMasterSrvNameLabel;
        private TabControl resultTabControl;
        private TabPage jsonViewerTabPage;
        private TabPage CSharpReadModelTabPage;
        private TextBox JsonViewerTextBox;
        private TextBox cSharpReadModelTextBox;
        private TabPage DomainTabPage;
        private TextBox domainTextBox;
        private TabPage TblFieldTabPage;
        private TextBox tblFieldTextBox;
    }
}