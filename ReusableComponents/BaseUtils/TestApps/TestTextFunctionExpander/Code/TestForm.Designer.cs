namespace Extract.BaseUtils.Testing
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this._buttonTest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._textSourceDoc = new System.Windows.Forms.TextBox();
            this._buttonClear = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this._textFpsFileDir = new System.Windows.Forms.TextBox();
            this._textValue = new System.Windows.Forms.TextBox();
            this._textExpansion = new System.Windows.Forms.TextBox();
            this._browseSourceDoc = new Extract.Utilities.Forms.BrowseButton();
            this._browseFpsDir = new Extract.Utilities.Forms.BrowseButton();
            this.pathTagsButton1 = new Extract.Utilities.Forms.PathTagsButton();
            this.SuspendLayout();
            // 
            // _buttonTest
            // 
            this._buttonTest.Location = new System.Drawing.Point(387, 360);
            this._buttonTest.Name = "_buttonTest";
            this._buttonTest.Size = new System.Drawing.Size(75, 23);
            this._buttonTest.TabIndex = 0;
            this._buttonTest.Text = "Test";
            this._buttonTest.UseVisualStyleBackColor = true;
            this._buttonTest.Click += new System.EventHandler(this.HandleTestButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Value";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 222);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Expansion";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "<SourceDocName>";
            // 
            // _textSourceDoc
            // 
            this._textSourceDoc.Location = new System.Drawing.Point(12, 25);
            this._textSourceDoc.Name = "_textSourceDoc";
            this._textSourceDoc.Size = new System.Drawing.Size(417, 20);
            this._textSourceDoc.TabIndex = 6;
            // 
            // _buttonClear
            // 
            this._buttonClear.Location = new System.Drawing.Point(12, 360);
            this._buttonClear.Name = "_buttonClear";
            this._buttonClear.Size = new System.Drawing.Size(75, 23);
            this._buttonClear.TabIndex = 9;
            this._buttonClear.Text = "Clear";
            this._buttonClear.UseVisualStyleBackColor = true;
            this._buttonClear.Click += new System.EventHandler(this.HandleButtonClearClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "<FPSFileDir>";
            // 
            // _textFpsFileDir
            // 
            this._textFpsFileDir.Location = new System.Drawing.Point(12, 64);
            this._textFpsFileDir.Name = "_textFpsFileDir";
            this._textFpsFileDir.Size = new System.Drawing.Size(417, 20);
            this._textFpsFileDir.TabIndex = 11;
            // 
            // _textValue
            // 
            this._textValue.Location = new System.Drawing.Point(12, 103);
            this._textValue.Multiline = true;
            this._textValue.Name = "_textValue";
            this._textValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textValue.Size = new System.Drawing.Size(417, 116);
            this._textValue.TabIndex = 12;
            // 
            // _textExpansion
            // 
            this._textExpansion.Location = new System.Drawing.Point(12, 238);
            this._textExpansion.Multiline = true;
            this._textExpansion.Name = "_textExpansion";
            this._textExpansion.ReadOnly = true;
            this._textExpansion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._textExpansion.Size = new System.Drawing.Size(450, 116);
            this._textExpansion.TabIndex = 13;
            // 
            // _browseSourceDoc
            // 
            this._browseSourceDoc.FileFilter = "All files (*.*)|*.*||";
            this._browseSourceDoc.Location = new System.Drawing.Point(435, 24);
            this._browseSourceDoc.Name = "_browseSourceDoc";
            this._browseSourceDoc.Size = new System.Drawing.Size(27, 20);
            this._browseSourceDoc.TabIndex = 14;
            this._browseSourceDoc.Text = "...";
            this._browseSourceDoc.TextControl = this._textSourceDoc;
            this._browseSourceDoc.UseVisualStyleBackColor = true;
            // 
            // _browseFpsDir
            // 
            this._browseFpsDir.FolderBrowser = true;
            this._browseFpsDir.Location = new System.Drawing.Point(435, 63);
            this._browseFpsDir.Name = "_browseFpsDir";
            this._browseFpsDir.Size = new System.Drawing.Size(27, 20);
            this._browseFpsDir.TabIndex = 15;
            this._browseFpsDir.Text = "...";
            this._browseFpsDir.TextControl = this._textFpsFileDir;
            this._browseFpsDir.UseVisualStyleBackColor = true;
            // 
            // pathTagsButton1
            // 
            this.pathTagsButton1.Image = ((System.Drawing.Image)(resources.GetObject("pathTagsButton1.Image")));
            this.pathTagsButton1.Location = new System.Drawing.Point(435, 102);
            this.pathTagsButton1.Name = "pathTagsButton1";
            this.pathTagsButton1.PathTags = new Extract.Utilities.FileActionManagerPathTags();
            this.pathTagsButton1.Size = new System.Drawing.Size(18, 20);
            this.pathTagsButton1.TabIndex = 16;
            this.pathTagsButton1.UseVisualStyleBackColor = true;
            this.pathTagsButton1.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandlePathTagSelected);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 394);
            this.Controls.Add(this.pathTagsButton1);
            this.Controls.Add(this._browseFpsDir);
            this.Controls.Add(this._browseSourceDoc);
            this.Controls.Add(this._textExpansion);
            this.Controls.Add(this._textValue);
            this.Controls.Add(this._textFpsFileDir);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._buttonClear);
            this.Controls.Add(this._textSourceDoc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._buttonTest);
            this.Name = "TestForm";
            this.Text = "Text Function Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _buttonTest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _textSourceDoc;
        private System.Windows.Forms.Button _buttonClear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _textFpsFileDir;
        private System.Windows.Forms.TextBox _textValue;
        private System.Windows.Forms.TextBox _textExpansion;
        private Extract.Utilities.Forms.BrowseButton _browseSourceDoc;
        private Extract.Utilities.Forms.BrowseButton _browseFpsDir;
        private Extract.Utilities.Forms.PathTagsButton pathTagsButton1;
    }
}

