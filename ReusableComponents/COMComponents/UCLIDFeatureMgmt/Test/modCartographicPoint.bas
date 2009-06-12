Attribute VB_Name = "modCartographicPoint"
Option Explicit
Public Sub testCartographicPoint()
    
    testSetCoordinates
    testValueIsEqualTo
        
End Sub

Private Sub testSetCoordinates()
    
    Dim p1 As ICartographicPoint
    Set p1 = New CartographicPoint
    
    Dim dX As Double
    Dim dY As Double
    dX = 4.2
    dY = 2.4
    p1.dX = dX
    p1.dY = dY
    
    ' test to see if dx attribute was successfully updated
    Dim strTestName As String
    strTestName = "CartographicPoint.dX"
    If dX <> p1.dX Then
        frmProgress.recordFailedTest strTestName, "ELI01524"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' test to see if the dy attribute was successfully updated
    strTestName = "CartographicPoint.dY"
    If dY <> p1.dY Then
        frmProgress.recordFailedTest strTestName, "ELI01525"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' cleanup
    Set p1 = Nothing

End Sub

Private Sub testValueIsEqualTo()
   
    Dim p1 As ICartographicPoint
    Dim p2 As ICartographicPoint
    Dim p3 As ICartographicPoint
    Set p1 = createRandomPoint()
    Set p2 = p1
    Set p3 = createRandomPoint
    Dim strTestName As String
    
    ' ensure that p1 and p2 are same
    strTestName = "CatrographicPoint.valueIsEqualTo() - 1"
    If (p1.valueIsEqualTo(p2)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01585"
    End If
    
    ' ensure that p2 and p3 are not same
    strTestName = "CatrographicPoint.valueIsEqualTo() - 2"
    If (p2.valueIsEqualTo(p3)) Then
        frmProgress.recordFailedTest strTestName, "ELI01586"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' cleanup
    Set p1 = Nothing
    Set p2 = Nothing
    Set p3 = Nothing
End Sub
