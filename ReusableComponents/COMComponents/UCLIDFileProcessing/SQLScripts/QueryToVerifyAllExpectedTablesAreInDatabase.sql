DECLARE @table_names xml;
DECLARE	@tableOfNames TABLE(ExpectedName nvarchar(max))

SELECT @table_names = CAST('<XMLRoot><ExpectedName>' + REPLACE(
'vUsersWithActive
,AttributeSetForFile
,Tag
,DashboardAttributeFields
,AttributeName
,UserCreatedCounter
,vFAMUserInputEventsTime
,SourceDocChangeHistory
,vPaginationDataWithRank
,AttributeType
,ReportingHIMStats
,DocTagHistory
,AttributeInstanceType
,DBInfoChangeHistory
,Attribute
,FTPAccount
,RasterZone
,FTPEventHistory
,QueuedActionStatusChange
,vFAMUserInputEventsTimeLegacy
,FileTaskSessionCache
,SecureCounter
,SecureCounterValueChange
,Pagination
,FieldSearch
,WorkflowType
,Workflow
,FileHandler
,WorkflowFile
,Action
,LockTable
,WorkflowChange
,ActionState
,WorkflowChangeFile
,FAMFile
,QueueEventCode
,ActionStatistics
,DataEntryCounterDefinition
,DataEntryCounterType
,DataEntryCounterValue
,MLModel
,MLData
,Login
,FileActionStateTransition
,ReportingDatabaseMigrationWizard
,WebAppConfig
,Feature
,QueueEvent
,Machine
,DatabaseService
,FAMUser
,vFAMUserInputWithFileID
,ReportingRedactionAccuracy
,IDShieldData
,WorkItemGroup
,FileActionComment
,WorkItem
,LabDEPatient
,SkippedFile
,LabDEOrderStatus
,MetadataField
,LabDEOrder
,FileTag
,FileMetadataFieldValue
,ActiveFAM
,ReportingDataCaptureAccuracy
,LockedFile
,LabDEOrderFile
,FPSFile
,LabDEEncounter
,FAMSession
,LabDEEncounterFile
,InputEvent
,LabDEPatientFile
,ReportingVerificationRates
,FileActionStatus
,LabDEProvider
,ActionStatisticsDelta
,TaskClass
,FileTaskSession
,DBInfo
,Dashboard
,AttributeSetName
,vPaginatedDestFiles
,vProcessingData', 
',', '</ExpectedName><ExpectedName>') + '</ExpectedName></XMLRoot>' AS XML) 

INSERT INTO @tableOfNames
	Select ltrim(Rtrim(Replace(Replace( a.r.query('.').value('.', 'nvarchar(max)'),char(13),''),char(10),''))) as ExpectedName from @Table_names.nodes('/XMLRoot/ExpectedName') a(r)


select ExpectedName, TABLE_NAME from @tableOfNames ET
full join 

(Select TABLE_NAME from INFORMATION_SCHEMA.TABLES ) as t 

ON et.ExpectedName = TABLE_NAME
WHERE  TABLE_NAME is null or ExpectedName is null

