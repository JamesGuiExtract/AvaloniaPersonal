VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ArcGISAttributeManager"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Implements IAttributeManager
Implements IArcGISDependentComponent

Private m_pApplication As esriFramework.IApplication
Private m_pEditor As esriEditor.IEditor
Private m_pSourceDocCollection As IVariantVector
Private m_pFeatureAttributeDataInterpreter As UCLID_FEATUREMGMTLib.IFeatureAttributeDataInterpreter


Private Sub Class_Initialize()
    Set m_pFeatureAttributeDataInterpreter = Nothing
End Sub

Private Sub Class_Terminate()
    Set m_pFeatureAttributeDataInterpreter = Nothing
End Sub

Private Sub IArcGISDependentComponent_setApplicationHook(hook As Object)
On Error GoTo ErrHandler
    Dim pId As New esriSystem.UID
    Set m_pApplication = hook
    'assign progId to the pId
    pId = "esriEditor.editor"
    'get the clsid
    Set m_pEditor = m_pApplication.FindExtensionByCLSID(pId)
    
Exit Sub
ErrHandler:
    HandleError "ELI01762"
End Sub

Private Function IAttributeManager_CanStoreAttributes(ByVal strFeatureID As String) As Boolean
On Error GoTo ErrHandler
    IAttributeManager_CanStoreAttributes = False
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Set pMxDoc = m_pApplication.Document
    'only get the feature that is currently selected
    If pMxDoc.FocusMap.SelectionCount = 1 Then
        Dim pEnumFeature As esriGeoDatabase.IEnumFeature
        Dim pFeature As esriGeoDatabase.IFeature
        Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
        ' get the selected feature
        Set pFeature = pEnumFeature.Next
        
        ' If selected feature class is a shapefile, no store attributes
        ' shall be enabled
        Dim pWorkspaceFactory As esriGeoDatabase.IWorkspaceFactory
        Dim strWorkspaceDescription As String
        Dim pDataset As esriGeoDatabase.IDataset
        Dim pWorkspace As esriGeoDatabase.IWorkspace
        Set pDataset = pFeature.Class
        Set pWorkspace = pDataset.Workspace
        Set pWorkspaceFactory = pWorkspace.WorkspaceFactory
        strWorkspaceDescription = pWorkspaceFactory.WorkspaceDescription(False)
        If strWorkspaceDescription = "Shapefiles" Then       'shapefile
            Exit Function
        End If
        
        Dim index As Integer
        index = FindIcoMapAttributeField(pFeature.Class)

        If index >= 0 Then
            IAttributeManager_CanStoreAttributes = True
        End If
    End If
Exit Function

ErrHandler:
    HandleError "ELI01975"
End Function

Private Function IAttributeManager_GetFeatureAttribute(ByVal strFeatureID As String) As UCLID_FEATUREMGMTLib.IFeature
On Error GoTo ErrHandler
    Dim pMxDoc As esriArcMapUI.IMxDocument
    Set pMxDoc = m_pApplication.Document
    'only get the feature that is currently selected
    If pMxDoc.FocusMap.SelectionCount = 1 Then
        Dim pEnumFeature As esriGeoDatabase.IEnumFeature
        Dim pFeature As esriGeoDatabase.IFeature
        Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
        ' get the selected feature
        Set pFeature = pEnumFeature.Next
        
        Dim index As Integer
        index = FindIcoMapAttributeField(pFeature.Class)

        If index < 0 Then
            Set IAttributeManager_GetFeatureAttribute = Nothing
        Else
            ' Get the uclid feature string from the field
            Dim strUCLIDFeatureAttributes As String
            strUCLIDFeatureAttributes = ""
            ' only get the feature string if it's not empty
            If pFeature.Value(index) <> Empty Then
                strUCLIDFeatureAttributes = pFeature.Value(index)
            End If
            
            ' trim off any leading or trailing white spaces
            strUCLIDFeatureAttributes = Trim(strUCLIDFeatureAttributes)
            
            If strUCLIDFeatureAttributes = "" Then
                Set IAttributeManager_GetFeatureAttribute = Nothing
            Else
                ' Using feature interpreter to convert the string into uclid IFeature
                Set IAttributeManager_GetFeatureAttribute = GetAttributeDataInterpreter().getFeatureFromAttributeData(strUCLIDFeatureAttributes)
            End If
        End If
    End If
Exit Function
ErrHandler:
    HandleError "ELI01763"
End Function
Private Function IAttributeManager_GetSegmentSourceDocuments(ByVal strSegmentID As String) As IVariantVector
'not implemented for ArcGIS platform
End Function
Private Function IAttributeManager_GetSketchSourceDocuments(ByVal strFeatureID As String) As IVariantVector
On Error GoTo ErrHandler
    If Not m_pSourceDocCollection Is Nothing Then
        Set IAttributeManager_GetSketchSourceDocuments = m_pSourceDocCollection
    Else
        Set IAttributeManager_GetSketchSourceDocuments = Nothing
    End If
Exit Function
ErrHandler:
    HandleError "ELI01764"

End Function

Private Sub IAttributeManager_SetFeatureAttribute(ByVal strFeatureID As String, ByVal ipIFeature As UCLID_FEATUREMGMTLib.IFeature)
On Error GoTo ErrHandler
    ' only if current task is create new feature
    If GetCurrentTaskName() = "Create New Feature" Then
        Dim pMxDoc As esriArcMapUI.IMxDocument
        Set pMxDoc = m_pApplication.Document
        'only get the feature that is currently selected
        If pMxDoc.FocusMap.SelectionCount = 1 Then
            Dim pEnumFeature As esriGeoDatabase.IEnumFeature
            Dim pFeature As esriGeoDatabase.IFeature
            Set pEnumFeature = pMxDoc.FocusMap.FeatureSelection
            ' get the selected feature
            Set pFeature = pEnumFeature.Next
            ' Using uclid feature interpreter to convert the string into uclid IFeature
            Dim strIcoMapFeatureAttributes As String
            
            'if the original feature is empty, remove all content from the attribute field
            If ipIFeature Is Nothing Then
                strIcoMapFeatureAttributes = ""
            Else
                strIcoMapFeatureAttributes = GetAttributeDataInterpreter().getAttributeDataFromFeature(ipIFeature)
            End If
            Dim pWorkspaceEdit As esriGeoDatabase.IWorkspaceEdit
            Dim pDataset As esriGeoDatabase.IDataset
            Dim pWorkspace As esriGeoDatabase.IWorkspace
            Dim pFeatureClass As esriGeoDatabase.IFeatureClass
            Set pFeatureClass = pFeature.Class
            Set pDataset = pFeatureClass
            Set pWorkspace = pDataset.Workspace
            Set pWorkspaceEdit = pWorkspace
            
            ' If current feature class is shape file, do not store any features
            Dim pWorkspaceFactory As esriGeoDatabase.IWorkspaceFactory
            Set pWorkspaceFactory = pWorkspace.WorkspaceFactory
            Dim strWorkspaceDescription As String
            strWorkspaceDescription = pWorkspaceFactory.WorkspaceDescription(False)
            If strWorkspaceDescription = "Shapefiles" Then       'shapefile
                Exit Sub
            End If
            
            ' only in edit mode can feature field be edited
            If pWorkspaceEdit.IsBeingEdited Then
                ' if the field exists
                Dim index As Long
                index = FindIcoMapAttributeField(pFeatureClass)
                If index >= 0 Then
                    ' read the actual icomapattr field length from ArcMap
                    Dim nLen As Long
                    nLen = GetIcoMapAttrFieldLength(pFeatureClass)
                    ' check length of feature string
                    If Len(strIcoMapFeatureAttributes) < nLen Then
                        m_pEditor.StartOperation
                        ' store the feature string into the feature class table
                        pFeature.Value(index) = strIcoMapFeatureAttributes
                        'commit the transaction
                        pFeature.Store
                        m_pEditor.StopOperation "Stored feature string database."
                    Else
                        ' present message to user
                        MsgBox "This feature attribute has exceeded the maximum length defined for IcoMapAttr field. " & _
                        "IcoMap will not store the original attribute for this feature.", vbExclamation
                    End If
                End If
            End If
        End If
    End If
Exit Sub
ErrHandler:
    HandleError "ELI01765"
End Sub

Private Sub IAttributeManager_SetSegmentSourceDocuments(ByVal strSegmentID As String, ByVal pColSrcDocStrings As IVariantVector)
'not implemented for ArcGIS platform
End Sub
Private Sub IAttributeManager_SetSketchSourceDocuments(ByVal strFeatureID As String, ByVal pColSrcDocStrings As IVariantVector)
On Error GoTo ErrHandler
    ' only if current task is create new feature
    If GetCurrentTaskName() = "Create New Feature" Then
        'when this sub is called from outside, we store all these source files to the hyperlink container
        Set m_pSourceDocCollection = pColSrcDocStrings
        
        'find current active layer, set hyperlink container
        Dim pEditLayers As esriEditor.IEditLayers
        Dim pFeatureLayer As esriCarto.IFeatureLayer
        Dim pHyperlinkContainer As esriCarto.IHyperlinkContainer
        Set pEditLayers = m_pEditor
        Set pFeatureLayer = pEditLayers.CurrentLayer
        Set pHyperlinkContainer = pFeatureLayer
        
        'convert the feature id from string to long
        Dim lFeatureID As Long
        If IsNumeric(strFeatureID) Then
            lFeatureID = Trim(strFeatureID)
        Else
            Dim ex As ICOMUCLIDException
            Set ex = New COMUCLIDException
            ex.CreateFromString "ELI03143", "Invalid feature ID."
            ex.AddDebugInfo "strFeatureID", strFeatureID
            ' convert the UCLIDException to a string and throw as a COM error with
            ' description
            Err.Raise vbObjectError, "", ex.AsStringizedByteStream
        End If
        
        Dim i As Integer
        For i = 0 To m_pSourceDocCollection.Size() - 1
            Dim pHyperlink As esriCarto.IHyperlink
            'create a new hyper link
            Set pHyperlink = New esriCarto.Hyperlink
            pHyperlink.LinkType = esriHyperlinkTypeDocument   'the link is document, not URL
            pHyperlink.Link = m_pSourceDocCollection.Item(i)  'store the item (i.e. the source doc) in the hyperlink
            pHyperlink.FeatureId = lFeatureID
            pHyperlinkContainer.AddHyperlink pHyperlink
        Next i
    End If
Exit Sub
ErrHandler:
    HandleError "ELI01766"
End Sub
Private Function FindIcoMapAttributeField(pFeatureClass As esriGeoDatabase.IFeatureClass) As Long
On Error GoTo ErrHandler
    Dim index As Long
    index = -1
    If pFeatureClass.ShapeType = esriGeometry.esriGeometryPolyline Or pFeatureClass.ShapeType = esriGeometryPolygon Then
         'create the field anyway if it's not there
        Dim strIcoMapAttributeField As String
        strIcoMapAttributeField = "IcoMapAttr"
         
        index = pFeatureClass.FindField(strIcoMapAttributeField)

    End If
    FindIcoMapAttributeField = index
    
Exit Function
ErrHandler:
    If Err.Number = -2147220961 Then
        Err.Description = Err.Description & " You don't have the permission to modify the database table"
    End If
    HandleError "ELI01777"
End Function
Private Function GetAttributeDataInterpreter() As UCLID_FEATUREMGMTLib.IFeatureAttributeDataInterpreter
    ' set the error handler
    On Error GoTo ErrHandler
    
    If (Not m_pFeatureAttributeDataInterpreter Is Nothing) Then
        Set GetAttributeDataInterpreter = m_pFeatureAttributeDataInterpreter
        Exit Function
    End If
    
    ' retrieve the data interpreter description string from the registry
    Dim strDataInterpreterDescription As String
    Dim lKey As Long
    Dim strKeyPath As String
    strKeyPath = "SOFTWARE\UCLID Software\IcoMap for ArcGIS\Options\General"
    Dim strKeyName As String
    strKeyName = "DataInterpreter"
    Dim strDefaultKeyValue As String
    strDefaultKeyValue = "Store text delimited with pipe & colon characters"
    Dim strKeyValue As String
    strKeyValue = GetSettingString(HKEY_LOCAL_MACHINE, strKeyPath, strKeyName)
    
    If (strKeyValue <> "") Then
        ' create an instance of the UCLID component manager to retrieve the correct
        ' feature attribute data interpreter
        Dim pCatMgr As New CategoryManager
        Dim pDescriptionToProgIDMap As IStrToStrMap
        Set pDescriptionToProgIDMap = pCatMgr.GetDescriptionToProgIDMap1("UCLID IFeatureAttributeDataInterpreter")
        
        ' ensure that the key value (description of the object) exists in the map
        If (Not pDescriptionToProgIDMap.Contains(strKeyValue)) Then
            Dim pEx1 As New COMUCLIDException
            pEx1.AddHistoryRecord "ELI04274", "Unable to find specified FeatureAttributeDataInterpreter!"
            pEx1.AddDebugInfo "Description", strKeyValue
            pEx1.Display
            Set GetAttributeDataInterpreter = Nothing
            Exit Function
        End If
        
        ' get the prog id of the component and instantiate it
        Dim strProgID As String
        strProgID = pDescriptionToProgIDMap.GetValue(strKeyValue)
        Set m_pFeatureAttributeDataInterpreter = CreateObject(strProgID)
        If (m_pFeatureAttributeDataInterpreter Is Nothing) Then
            Dim pEx2 As New COMUCLIDException
			pEx2.AddHistoryRecord("ELI19494", "Unable to instantiate specified FeatureAttributeDataInterpreter!")
            pEx2.AddDebugInfo "Description", strKeyValue
            pEx2.AddDebugInfo "ProgID", strProgID
            pEx2.Display
            Set GetAttributeDataInterpreter = Nothing
            Exit Function
        End If
    Else
        ' the key was not found in the registry...use the default
        ' feature attribute data interpreter
        Set m_pFeatureAttributeDataInterpreter = New UCLID_FEATUREMGMTLib.CommaDelimitedFeatureAttributeDataInterpreter
    End If
    
    ' set the use of the default feature attribute data interpreter
    Set GetAttributeDataInterpreter = m_pFeatureAttributeDataInterpreter
    Exit Function
    
ErrHandler:
    HandleError "ELI02155"
End Function
Private Function GetIcoMapAttrFieldLength(pFeatureClass As esriGeoDatabase.IFeatureClass) As Long
On Error GoTo ErrHandler
    GetIcoMapAttrFieldLength = 0
    Dim index As Long
    index = FindIcoMapAttributeField(pFeatureClass)
    If index >= 0 Then
        Dim pFields As esriGeoDatabase.IFields
        Dim pField As esriGeoDatabase.IField
        Set pFields = pFeatureClass.Fields
        Set pField = pFields.Field(index)
        If Not pField Is Nothing Then
            GetIcoMapAttrFieldLength = pField.Length
        End If
    End If
Exit Function
ErrHandler:
    HandleError "ELI03889"
End Function
Private Function GetCurrentTaskName() As String
On Error GoTo ErrHandler
    GetCurrentTaskName = ""
    Dim ipEditTask As esriEditor.IEditTask
    Set ipEditTask = m_pEditor.CurrentTask
    If Not ipEditTask Is Nothing Then
        GetCurrentTaskName = ipEditTask.Name
    End If
Exit Function
ErrHandler:
    HandleError "ELI04136"
End Function
Private Sub HandleError(ByVal strELI As String)
    'create a COM UCLIDException object
    Dim ex As ICOMUCLIDException
    Set ex = New COMUCLIDException
    ex.CreateFromString strELI, Err.Description
    ex.AddDebugInfo "Error Number", Err.Number
    
    ' convert the UCLIDException to a string and throw as a COM error with
    ' description
    Dim strMsg As String
    strMsg = ex.AsStringizedByteStream
    Err.Raise vbObjectError, "", strMsg
End Sub

