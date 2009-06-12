Attribute VB_Name = "modPart"
Option Explicit
Public Sub testPart()
    
    testStartingPoint
    testSegments
    
End Sub

Private Sub testStartingPoint()

    'create a random part object
    Dim pPart As IPart
    Dim pStartPoint As ICartographicPoint
    Dim pSegments As IIUnknownVector
    Set pPart = createRandomPart(pSegments, pStartPoint)
    
    'test to see if the retrieved starting point is correct
    Dim pRetrievedStartPoint As ICartographicPoint
    Set pRetrievedStartPoint = pPart.getStartingPoint
    Dim strTestName As String
    strTestName = "Part.getStartingPoint()"
    If (pRetrievedStartPoint.valueIsEqualTo(pStartPoint)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01553"
    End If

End Sub

Private Sub testSegments()
    
    'create a random part object
    Dim pPart As IPart
    Dim pStartPoint As ICartographicPoint
    Dim pSegments As IIUnknownVector
    Set pPart = createRandomPart(pSegments, pStartPoint)
        
    ' get the segments from the part and see if they
    ' are what they should be
    Dim pEnumSegments As IEnumSegment
    Set pEnumSegments = pPart.getSegments
    Dim pRetrievedSegment As ISegment
    
    Dim i As Integer
    pEnumSegments.Reset
    For i = 0 To pSegments.Size - 1
        Dim pSegment As ISegment
        Set pSegment = pEnumSegments.Next
        Dim pExpectedSegment As ISegment
        Set pExpectedSegment = pSegments.at(i)
        Set pRetrievedSegment = pSegment
        
        'test to make sure that the segments are of the same type
        Dim strTestName As String
        strTestName = "Part.getSegments().at(" & i & ").getSegmentType()"
        If (pExpectedSegment.getSegmentType <> pRetrievedSegment.getSegmentType) Then
            frmProgress.recordFailedTest strTestName, "ELI01555"
        Else
            frmProgress.recordPassedTest strTestName
        End If
        
        'test to make sure that the segment data is the same
        If (pExpectedSegment.getSegmentType = kArc) Then
            Dim pExpectedArc As IArcSegment
            Set pExpectedArc = pExpectedSegment 'QI
            Dim pRetrievedArc  As IArcSegment
            Set pRetrievedArc = pRetrievedSegment 'QI
            strTestName = "(ArcSegment) Part.getSegments().at(" & i & ")"
            If (pRetrievedArc.valueIsEqualTo(pExpectedArc)) Then
                frmProgress.recordPassedTest strTestName
            Else
                frmProgress.recordFailedTest strTestName, "ELI01572"
            End If
        ElseIf (pExpectedSegment.getSegmentType = kLine) Then
            Dim pExpectedLine As ILineSegment
            Set pExpectedLine = pExpectedSegment 'QI
            Dim pRetrievedLine  As ILineSegment
            Set pRetrievedLine = pRetrievedSegment 'QI
            strTestName = "(LineSegment) Part.getSegments().at(" & i & ")"
            If (pRetrievedLine.valueIsEqualTo(pExpectedLine)) Then
                frmProgress.recordPassedTest strTestName
            Else
				frmProgress.recordFailedTest(strTestName, "ELI19491")
            End If
        End If
    Next i
       
    'ensure that there is no more segments
    Set pSegment = pEnumSegments.Next
    If (Not pSegment Is Nothing) Then
        frmProgress.recordFailedTest "???", "???"
    Else
        frmProgress.recordPassedTest "???"
    End If

End Sub
