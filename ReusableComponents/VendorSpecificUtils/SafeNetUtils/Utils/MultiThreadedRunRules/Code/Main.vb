Public Class Main

	' the total number of files the user wishes to process
	Private Const nNumberOfFilesToProcess As Integer = 250000

	Private strInputFolder As String
	Private strRulesFile As String
	Private nNumberOfPasses As Integer
	Private bExitApplication As Boolean
	Private Shared stackFiles As Stack(Of String)
	Private Shared nThreadCount As Integer
	Private Shared objLock As New Object
	Private threads() As System.Threading.Thread
	Private Files As String()

	Private Sub btnExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExit.Click
		bExitApplication = True
		Application.Exit()
	End Sub

	Private Sub btnRun_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRun.Click
		Dim strNumThreads As String
		Dim nNumThreads As Integer

		' be sure an input folder has been selected
		If strInputFolder = "" Then
			MsgBox("Please select an input folder!")
			Exit Sub
		End If

		If strRulesFile = "" Then
			MsgBox("Please select a rules file to run!")
			Exit Sub
		End If

		' get the number of threads from the dialog box
		strNumThreads = txtNumThreads.Text

		If Not IsNumeric(strNumThreads) Then
			MsgBox("Number of threads must be a numberic value! " _
			   & "NumThreads: " & strNumThreads, MsgBoxStyle.OkOnly)
			txtNumThreads.Focus()
			Exit Sub
		End If

		' convert the string to integer
		nNumThreads = CInt(strNumThreads)

		If nNumThreads <= 0 Then
			MsgBox("Number of threads must be greater than 0! " _
			& "NumThreads: " & strNumThreads, MsgBoxStyle.OkOnly)
			txtNumThreads.Focus()
			Exit Sub
		End If

		nThreadCount = 0
		progressPass.Minimum = 1
		progressPass.Maximum = Files.Length
		progressPass.Step = 1
		progressOverAll.Minimum = 1
		progressOverAll.Maximum = nNumberOfPasses * Files.Length
		progressOverAll.Value = 1
		progressOverAll.Step = 1

		stackFiles = New Stack(Of String)
		ReDim threads(nNumThreads - 1)

		' loop through the files nNumberOfPasses times
		' this should process around 150,000 files
		For i As Integer = 1 To nNumberOfPasses
			' new pass, set progress bar back to 1
			progressPass.Value = 1
			txtCurrentPass.Text = CStr(i)
			Me.Refresh()

			' refill the stack of files
			For Each file As String In Files
				stackFiles.Push(file)
			Next

			' loop while there are still files to process in the stack
			While stackFiles.Count > 0
				' while there are less than the maximum number of threads
				' launch a new thread
				While (nThreadCount < nNumThreads And Not bExitApplication)
					' lock this section of code with a SyncLock
					SyncLock objLock
						' create a new thread
						threads(nThreadCount) = New System.Threading.Thread(AddressOf ProcessFile)
						threads(nThreadCount).IsBackground = True

						' update the progress bars
						progressOverAll.PerformStep()
						progressPass.PerformStep()

						' launch the thread
						threads(nThreadCount).Start()

						' increment the number of current threads
						nThreadCount += 1
					End SyncLock
					' refresh the dialog
					Me.Refresh()
					' make sure events get processed
					Application.DoEvents()
				End While

				If bExitApplication Then
					Exit Sub
				End If
			End While
		Next

	End Sub

	Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
		bExitApplication = True
		' wait for the threads to finish
		While (nThreadCount > 0)
			Application.DoEvents()
		End While
		Application.Exit()
	End Sub

	Private Sub ChooseInputFolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChooseInputFolderToolStripMenuItem.Click
		Dim fb As New FolderBrowserDialog
		fb.SelectedPath = Application.LocalUserAppDataPath
		fb.Description = "Please select an input folder."
		fb.ShowNewFolderButton = False
		If fb.ShowDialog() = Windows.Forms.DialogResult.OK Then
			strInputFolder = fb.SelectedPath
			Files = System.IO.Directory.GetFiles(strInputFolder)
			txtNumberFiles.Text = CStr(Files.Length)
			nNumberOfPasses = Math.Ceiling(CDbl(nNumberOfFilesToProcess) / CDbl(Files.Length))
			txtNumberOfPasses.Text = CStr(nNumberOfPasses)
		Else
			strInputFolder = ""
			ReDim Files(0)
			nNumberOfPasses = 0
			txtNumberFiles.Text = "0"
			txtNumberOfPasses.Text = "0"
		End If

		txtFolder.Text = strInputFolder

	End Sub

	Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) _
	 Handles MyBase.Load
		strInputFolder = ""
		strRulesFile = ""
		nThreadCount = 0
		nNumberOfPasses = 0
		ReDim threads(0)
		bExitApplication = False
	End Sub

	Private Sub ProcessFile()
		Dim fileName As String = ""

		' lock this section of code with a SyncLock
		SyncLock objLock
			' get a file name from the stack
			If stackFiles.Count > 0 Then
				fileName = stackFiles.Pop()
			End If
		End SyncLock

		' check to be sure file name is not empty
		If fileName <> "" Then
			' create a new process object
			Dim myProcess As New Process

			' set the process object
			myProcess.StartInfo.FileName = Application.StartupPath & "\RunRules.exe"
			myProcess.StartInfo.Arguments = Chr(34) & strRulesFile & Chr(34) & " " & Chr(34) _
			 & fileName & Chr(34) & " /i"
			myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden

			' start the process and wait for it to exit
			myProcess.Start()
			myProcess.WaitForExit()
		End If

		' lock this section of code with a SyncLock
		SyncLock objLock
			' thread is done, decrement the currently running thread count
			nThreadCount -= 1
		End SyncLock
	End Sub

	Private Sub ChooseRulesFileToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChooseRulesFileToolStripMenuItem.Click

		' open a file browser to browse to a rule set
		Dim fb As New OpenFileDialog
		fb.Filter = "Rules files (*.rsd, *.rsd.etf)|*.rsd;*.rsd.etf"
		fb.Title = "Please select a rules file"
		fb.InitialDirectory = Application.LocalUserAppDataPath
		fb.Multiselect = False
		fb.CheckFileExists = True

		' show the file browser and check for file selection
		If fb.ShowDialog = Windows.Forms.DialogResult.OK Then
			strRulesFile = fb.FileName
		Else
			strRulesFile = ""
		End If

		' fill in the text box with the rules file name
		txtRulesFile.Text = strRulesFile

	End Sub
End Class