namespace TestTextFuntionExpander
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
            this._buttonTest = new System.Windows.Forms.Button();
            this._listValues = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._listExpansion = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this._textSourceDoc = new System.Windows.Forms.TextBox();
            this._buttonAdd = new System.Windows.Forms.Button();
            this._buttonModify = new System.Windows.Forms.Button();
            this._buttonClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _buttonTest
            // 
            this._buttonTest.Location = new System.Drawing.Point(626, 347);
            this._buttonTest.Name = "_buttonTest";
            this._buttonTest.Size = new System.Drawing.Size(75, 23);
            this._buttonTest.TabIndex = 0;
            this._buttonTest.Text = "Test";
            this._buttonTest.UseVisualStyleBackColor = true;
            this._buttonTest.Click += new System.EventHandler(this.HandleTestButtonClick);
            // 
            // _listValues
            // 
            this._listValues.FormattingEnabled = true;
            this._listValues.Location = new System.Drawing.Point(11, 64);
            this._listValues.Name = "_listValues";
            this._listValues.ScrollAlwaysVisible = true;
            this._listValues.Size = new System.Drawing.Size(327, 277);
            this._listValues.TabIndex = 1;
            this._listValues.SelectedIndexChanged += new System.EventHandler(this.HandleSelectedIndexChanged);
            this._listValues.DoubleClick += new System.EventHandler(this.HandleValueListDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Values";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(374, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Expansion";
            // 
            // _listExpansion
            // 
            this._listExpansion.FormattingEnabled = true;
            this._listExpansion.Location = new System.Drawing.Point(373, 64);
            this._listExpansion.Name = "_listExpansion";
            this._listExpansion.ScrollAlwaysVisible = true;
            this._listExpansion.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this._listExpansion.Size = new System.Drawing.Size(327, 277);
            this._listExpansion.TabIndex = 3;
            this._listExpansion.TabStop = false;
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
            this._textSourceDoc.Size = new System.Drawing.Size(689, 20);
            this._textSourceDoc.TabIndex = 6;
            // 
            // _buttonAdd
            // 
            this._buttonAdd.Location = new System.Drawing.Point(11, 347);
            this._buttonAdd.Name = "_buttonAdd";
            this._buttonAdd.Size = new System.Drawing.Size(75, 23);
            this._buttonAdd.TabIndex = 7;
            this._buttonAdd.Text = "Add";
            this._buttonAdd.UseVisualStyleBackColor = true;
            this._buttonAdd.Click += new System.EventHandler(this.HandleButtonAddClick);
            // 
            // _buttonModify
            // 
            this._buttonModify.Location = new System.Drawing.Point(92, 347);
            this._buttonModify.Name = "_buttonModify";
            this._buttonModify.Size = new System.Drawing.Size(75, 23);
            this._buttonModify.TabIndex = 8;
            this._buttonModify.Text = "Modify";
            this._buttonModify.UseVisualStyleBackColor = true;
            this._buttonModify.Click += new System.EventHandler(this.HandleModifyButtonClick);
            // 
            // _buttonClear
            // 
            this._buttonClear.Location = new System.Drawing.Point(173, 347);
            this._buttonClear.Name = "_buttonClear";
            this._buttonClear.Size = new System.Drawing.Size(75, 23);
            this._buttonClear.TabIndex = 9;
            this._buttonClear.Text = "Clear";
            this._buttonClear.UseVisualStyleBackColor = true;
            this._buttonClear.Click += new System.EventHandler(this.HandleButtonClearClick);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 375);
            this.Controls.Add(this._buttonClear);
            this.Controls.Add(this._buttonModify);
            this.Controls.Add(this._buttonAdd);
            this.Controls.Add(this._textSourceDoc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._listExpansion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._listValues);
            this.Controls.Add(this._buttonTest);
            this.Name = "TestForm";
            this.Text = "Text Function Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _buttonTest;
        private System.Windows.Forms.ListBox _listValues;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox _listExpansion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _textSourceDoc;
        private System.Windows.Forms.Button _buttonAdd;
        private System.Windows.Forms.Button _buttonModify;
        private System.Windows.Forms.Button _buttonClear;
    }
}

