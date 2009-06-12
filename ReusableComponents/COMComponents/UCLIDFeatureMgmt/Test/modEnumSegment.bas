Attribute VB_Name = "modEnumSegment"
Option Explicit
Public Sub testEnumSegment()
    
    ' create a random line segment
    Dim pLine As ILineSegment
    Dim strLineBearing As String
    Dim strLineDistance As String
    Dim pLineEndPoint As ICartographicPoint
    Set pLine = createRandomLineSegment(strLineBearing, strLineDistance, pLineEndPoint)
    
    ' create a random arc segment
    Dim pArc As IArcSegment
    Dim pArcParams As IIUnknownVector
    Dim pArcMidPoint As ICartographicPoint
    Dim pArcEndPoint As ICartographicPoint
    Set pArc = createRandomArcSegment(pArcParams, pArcMidPoint, pArcEndPoint)
    
    ' create a new enumsegment object
    Dim pEnumSegment As IEnumSegment
    Set pEnumSegment = New EnumSegment
    
    ' get the modifier interface
    Dim pEnumSegmentModifier As IEnumSegmentModifier
    Set pEnumSegmentModifier = pEnumSegment 'QI
    
    ' add the line and the arc segments to the enumsegment
    pEnumSegmentModifier.addSegment pLine
    pEnumSegmentModifier.addSegment pArc
    
    ' reset the enumerator and ensure that the first segment
    ' is the line segment we added above
    Dim strTestName As String
    strTestName = "EnumSegment.next().getSegmentType()"
    pEnumSegment.Reset
    Dim pSegment As ISegment
    Set pSegment = pEnumSegment.Next
    If (pSegment.getSegmentType <> kLine) Then
        frmProgress.recordFailedTest strTestName, "ELI01548"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    Dim pRetrievedLine As ILineSegment
    Set pRetrievedLine = pSegment 'QI
    strTestName = "(LineSegment) (EnumSegment.next())"
    If (pRetrievedLine.valueIsEqualTo(pLine)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01549"
    End If
    
    ' ensure that the second segment is the arc segment we added above
    Set pSegment = pEnumSegment.Next
    strTestName = "EnumSegment.next().getSegmentType()"
    If (pSegment.getSegmentType <> kArc) Then
        frmProgress.recordFailedTest strTestName, "ELI01551"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    Dim pRetrievedArc As IArcSegment
    Set pRetrievedArc = pSegment 'QI
    strTestName = "(ArcSegment) (EnumSegment.next())"
    If (pRetrievedArc.valueIsEqualTo(pArc)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01571"
    End If
    
    ' ensure that there are no more segments
    Set pSegment = pEnumSegment.Next
    strTestName = "EnumSegment.next()"
    If (Not pSegment Is Nothing) Then
        frmProgress.recordFailedTest strTestName, "ELI01552"
    Else
        frmProgress.recordPassedTest strTestName
    End If
End Sub
