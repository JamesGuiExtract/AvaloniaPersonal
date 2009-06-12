' This file can be used to test the functioning ouf the UCLIDCOMUtils.COMMutex object
' When testing run this file once when the "Wait" dialog comes up run it again without 
' clicking the ok button on the dialog. After the running the second time click on the 
' Ok button on the dialog, it should dismiss the dialog and the replace it with another
' identical dialog.  If it is not working there will be 2 dialogs displayed at the same time

Dim MutexObject 

Set MutexObject = CreateObject( "UCLIDCOMUtils.COMMutex")

MutexObject.CreateNamed "Mutex"
MutexObject.Acquire
MsgBox "wait Here", ,"Wait"
MutexObject.ReleaseNamedMutex

