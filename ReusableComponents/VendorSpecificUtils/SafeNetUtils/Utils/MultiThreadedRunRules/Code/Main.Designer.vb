<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.Label1 = New System.Windows.Forms.Label
		Me.txtNumThreads = New System.Windows.Forms.TextBox
		Me.btnRun = New System.Windows.Forms.Button
		Me.btnExit = New System.Windows.Forms.Button
		Me.MenuStrip1 = New System.Windows.Forms.MenuStrip
		Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
		Me.ChooseInputFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
		Me.ChooseRulesFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
		Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
		Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
		Me.Label2 = New System.Windows.Forms.Label
		Me.txtFolder = New System.Windows.Forms.TextBox
		Me.txtRulesFile = New System.Windows.Forms.TextBox
		Me.txtNumberFiles = New System.Windows.Forms.TextBox
		Me.Label3 = New System.Windows.Forms.Label
		Me.txtCurrentPass = New System.Windows.Forms.TextBox
		Me.progressPass = New System.Windows.Forms.ProgressBar
		Me.progressOverAll = New System.Windows.Forms.ProgressBar
		Me.Label4 = New System.Windows.Forms.Label
		Me.txtNumberOfPasses = New System.Windows.Forms.TextBox
		Me.MenuStrip1.SuspendLayout()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 35)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(94, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Number of threads"
		'
		'txtNumThreads
		'
		Me.txtNumThreads.Location = New System.Drawing.Point(12, 51)
		Me.txtNumThreads.Name = "txtNumThreads"
		Me.txtNumThreads.Size = New System.Drawing.Size(40, 20)
		Me.txtNumThreads.TabIndex = 1
		'
		'btnRun
		'
		Me.btnRun.Location = New System.Drawing.Point(12, 129)
		Me.btnRun.Name = "btnRun"
		Me.btnRun.Size = New System.Drawing.Size(55, 23)
		Me.btnRun.TabIndex = 2
		Me.btnRun.Text = "Run"
		Me.btnRun.UseVisualStyleBackColor = True
		'
		'btnExit
		'
		Me.btnExit.Location = New System.Drawing.Point(73, 129)
		Me.btnExit.Name = "btnExit"
		Me.btnExit.Size = New System.Drawing.Size(55, 23)
		Me.btnExit.TabIndex = 3
		Me.btnExit.Text = "Exit"
		Me.btnExit.UseVisualStyleBackColor = True
		'
		'MenuStrip1
		'
		Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
		Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
		Me.MenuStrip1.Name = "MenuStrip1"
		Me.MenuStrip1.Size = New System.Drawing.Size(346, 24)
		Me.MenuStrip1.TabIndex = 4
		Me.MenuStrip1.Text = "MenuStrip1"
		'
		'FileToolStripMenuItem
		'
		Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ChooseInputFolderToolStripMenuItem, Me.ChooseRulesFileToolStripMenuItem, Me.ToolStripSeparator1, Me.ExitToolStripMenuItem})
		Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
		Me.FileToolStripMenuItem.Size = New System.Drawing.Size(35, 20)
		Me.FileToolStripMenuItem.Text = "&File"
		'
		'ChooseInputFolderToolStripMenuItem
		'
		Me.ChooseInputFolderToolStripMenuItem.Name = "ChooseInputFolderToolStripMenuItem"
		Me.ChooseInputFolderToolStripMenuItem.Size = New System.Drawing.Size(179, 22)
		Me.ChooseInputFolderToolStripMenuItem.Text = "&Choose input folder"
		'
		'ChooseRulesFileToolStripMenuItem
		'
		Me.ChooseRulesFileToolStripMenuItem.Name = "ChooseRulesFileToolStripMenuItem"
		Me.ChooseRulesFileToolStripMenuItem.Size = New System.Drawing.Size(179, 22)
		Me.ChooseRulesFileToolStripMenuItem.Text = "C&hoose rules file"
		'
		'ToolStripSeparator1
		'
		Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
		Me.ToolStripSeparator1.Size = New System.Drawing.Size(176, 6)
		'
		'ExitToolStripMenuItem
		'
		Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
		Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(179, 22)
		Me.ExitToolStripMenuItem.Text = "&Exit"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(112, 35)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(77, 13)
		Me.Label2.TabIndex = 5
		Me.Label2.Text = "Number of files"
		'
		'txtFolder
		'
		Me.txtFolder.Location = New System.Drawing.Point(12, 77)
		Me.txtFolder.Name = "txtFolder"
		Me.txtFolder.ReadOnly = True
		Me.txtFolder.Size = New System.Drawing.Size(185, 20)
		Me.txtFolder.TabIndex = 6
		Me.txtFolder.TabStop = False
		'
		'txtRulesFile
		'
		Me.txtRulesFile.Location = New System.Drawing.Point(12, 103)
		Me.txtRulesFile.Name = "txtRulesFile"
		Me.txtRulesFile.ReadOnly = True
		Me.txtRulesFile.Size = New System.Drawing.Size(185, 20)
		Me.txtRulesFile.TabIndex = 7
		Me.txtRulesFile.TabStop = False
		'
		'txtNumberFiles
		'
		Me.txtNumberFiles.Location = New System.Drawing.Point(115, 51)
		Me.txtNumberFiles.Name = "txtNumberFiles"
		Me.txtNumberFiles.ReadOnly = True
		Me.txtNumberFiles.Size = New System.Drawing.Size(82, 20)
		Me.txtNumberFiles.TabIndex = 8
		Me.txtNumberFiles.TabStop = False
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(200, 77)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(67, 13)
		Me.Label3.TabIndex = 9
		Me.Label3.Text = "Current Pass"
		'
		'txtCurrentPass
		'
		Me.txtCurrentPass.Location = New System.Drawing.Point(203, 93)
		Me.txtCurrentPass.Name = "txtCurrentPass"
		Me.txtCurrentPass.ReadOnly = True
		Me.txtCurrentPass.Size = New System.Drawing.Size(34, 20)
		Me.txtCurrentPass.TabIndex = 10
		Me.txtCurrentPass.TabStop = False
		'
		'progressPass
		'
		Me.progressPass.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
		Me.progressPass.Location = New System.Drawing.Point(243, 93)
		Me.progressPass.Name = "progressPass"
		Me.progressPass.Size = New System.Drawing.Size(87, 21)
		Me.progressPass.TabIndex = 11
		'
		'progressOverAll
		'
		Me.progressOverAll.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
		Me.progressOverAll.Location = New System.Drawing.Point(203, 131)
		Me.progressOverAll.Name = "progressOverAll"
		Me.progressOverAll.Size = New System.Drawing.Size(127, 21)
		Me.progressOverAll.TabIndex = 12
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(200, 35)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(92, 13)
		Me.Label4.TabIndex = 13
		Me.Label4.Text = "Number of passes"
		'
		'txtNumberOfPasses
		'
		Me.txtNumberOfPasses.Location = New System.Drawing.Point(203, 51)
		Me.txtNumberOfPasses.Name = "txtNumberOfPasses"
		Me.txtNumberOfPasses.ReadOnly = True
		Me.txtNumberOfPasses.Size = New System.Drawing.Size(34, 20)
		Me.txtNumberOfPasses.TabIndex = 14
		Me.txtNumberOfPasses.TabStop = False
		'
		'Main
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(346, 166)
		Me.Controls.Add(Me.txtNumberOfPasses)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.progressOverAll)
		Me.Controls.Add(Me.progressPass)
		Me.Controls.Add(Me.txtCurrentPass)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.txtNumberFiles)
		Me.Controls.Add(Me.txtRulesFile)
		Me.Controls.Add(Me.txtFolder)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.btnExit)
		Me.Controls.Add(Me.btnRun)
		Me.Controls.Add(Me.txtNumThreads)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.MenuStrip1)
		Me.MainMenuStrip = Me.MenuStrip1
		Me.Name = "Main"
		Me.Text = "RunRules"
		Me.MenuStrip1.ResumeLayout(False)
		Me.MenuStrip1.PerformLayout()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents txtNumThreads As System.Windows.Forms.TextBox
	Friend WithEvents btnRun As System.Windows.Forms.Button
	Friend WithEvents btnExit As System.Windows.Forms.Button
	Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
	Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents ChooseInputFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
	Friend WithEvents ChooseRulesFileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents Label2 As System.Windows.Forms.Label
	Friend WithEvents txtFolder As System.Windows.Forms.TextBox
	Friend WithEvents txtRulesFile As System.Windows.Forms.TextBox
	Friend WithEvents txtNumberFiles As System.Windows.Forms.TextBox
	Friend WithEvents Label3 As System.Windows.Forms.Label
	Friend WithEvents txtCurrentPass As System.Windows.Forms.TextBox
	Friend WithEvents progressPass As System.Windows.Forms.ProgressBar
	Friend WithEvents progressOverAll As System.Windows.Forms.ProgressBar
	Friend WithEvents Label4 As System.Windows.Forms.Label
	Friend WithEvents txtNumberOfPasses As System.Windows.Forms.TextBox

End Class
