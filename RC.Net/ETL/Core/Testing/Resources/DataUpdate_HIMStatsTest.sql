/*
This script was created by Visual Studio on 8/28/2021 at 2:30 PM.
Run this script on VOYAGER.Demo_HIM_New (EXTRACT\la_william_parr) to make it the same as VOYAGER.Demo_HIM (EXTRACT\la_william_parr).
This script performs its actions in the following order:
1. Disable foreign-key constraints.
2. Perform DELETE commands. 
3. Perform UPDATE commands.
4. Perform INSERT commands.
5. Re-enable foreign-key constraints.
Please back up your target database before running this script.

SET NUMERIC_ROUNDABORT OFF
GO
SET XACT_ABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
*/
/*Pointer used for text / image updates. This might not be needed, but is declared here just in case*/
DECLARE @pv binary(16)
BEGIN TRANSACTION
ALTER TABLE [dbo].[WorkflowChange] DROP CONSTRAINT [FK_WorkflowChange_WorkflowDest]
ALTER TABLE [dbo].[ActionStatisticsDelta] DROP CONSTRAINT [FK_ActionStatisticsDelta_Action]
ALTER TABLE [dbo].[WorkflowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_FAMFile]
ALTER TABLE [dbo].[WorkflowChangeFile] DROP CONSTRAINT [FK_WorkflowChangeFile_WorkflowChange]
ALTER TABLE [Security].[GroupReport] DROP CONSTRAINT [FK_GroupReport_Group_ID]
ALTER TABLE [dbo].[ActionStatistics] DROP CONSTRAINT [FK_Statistics_Action]
ALTER TABLE [Security].[LoginGroupMembership] DROP CONSTRAINT [FK_LoginGroupMembership_Group_ID]
ALTER TABLE [Security].[LoginGroupMembership] DROP CONSTRAINT [FK_LoginGroupMembership_LOGIN_ID]
ALTER TABLE [dbo].[FileHandler] DROP CONSTRAINT [FK_FileHandler_Workflow]
ALTER TABLE [dbo].[WorkItemGroup] DROP CONSTRAINT [FK_WorkItemGroup_Action]
ALTER TABLE [dbo].[WorkItemGroup] DROP CONSTRAINT [FK_WorkItemGroup_FAMFile]
ALTER TABLE [dbo].[WorkItemGroup] DROP CONSTRAINT [FK_WorkItemGroup_FAMSession]
ALTER TABLE [dbo].[DBInfoChangeHistory] DROP CONSTRAINT [FK_DBInfoChangeHistory_User]
ALTER TABLE [dbo].[DBInfoChangeHistory] DROP CONSTRAINT [FK_DBInfoChangeHistory_Machine]
ALTER TABLE [dbo].[DBInfoChangeHistory] DROP CONSTRAINT [FK_DBInfoChageHistory_DBInfo]
ALTER TABLE [dbo].[WorkItem] DROP CONSTRAINT [FK_WorkItem_WorkItemGroup]
ALTER TABLE [dbo].[WorkItem] DROP CONSTRAINT [FK_WorkItem_FAMSession]
ALTER TABLE [Security].[GroupAction] DROP CONSTRAINT [FK_GroupAction_Group_ID]
ALTER TABLE [Security].[GroupAction] DROP CONSTRAINT [FK_GroupAction_Action_ID]
ALTER TABLE [dbo].[FTPEventHistory] DROP CONSTRAINT [FK_FTPEventHistory_FTPAccount]
ALTER TABLE [dbo].[FTPEventHistory] DROP CONSTRAINT [FK_FTPEventHistory_FAMFile]
ALTER TABLE [dbo].[FTPEventHistory] DROP CONSTRAINT [FK_FTPEventHistory_Action]
ALTER TABLE [dbo].[FTPEventHistory] DROP CONSTRAINT [FK_FTPEventHistory_Machine]
ALTER TABLE [dbo].[FTPEventHistory] DROP CONSTRAINT [FK_FTPEventHistory_FAMUser]
ALTER TABLE [dbo].[IDShieldData] DROP CONSTRAINT [FK_IDShieldData_FileTaskSession]
ALTER TABLE [dbo].[FileMetadataFieldValue] DROP CONSTRAINT [FK_FileMetadataFieldValue_FAMFile]
ALTER TABLE [dbo].[FileMetadataFieldValue] DROP CONSTRAINT [FK_FileMetadataFieldValue_MetadataField]
ALTER TABLE [dbo].[FileTag] DROP CONSTRAINT [FK_FileTag_FamFile]
ALTER TABLE [dbo].[FileTag] DROP CONSTRAINT [FK_FileTag_Tag]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Expected]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Found]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_FAMFile]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_DatabaseService]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_FAMUser_Found]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_FAMUser_Expected]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_Action_Found]
ALTER TABLE [dbo].[ReportingRedactionAccuracy] DROP CONSTRAINT [FK_ReportingRedactionAccuracy_Action_Expected]
ALTER TABLE [dbo].[LabDEEncounterFile] DROP CONSTRAINT [FK_EncounterFile_EncounterID]
ALTER TABLE [dbo].[LabDEEncounterFile] DROP CONSTRAINT [FK_EncounterFile_FAMFile]
ALTER TABLE [dbo].[FileTaskSessionCache] DROP CONSTRAINT [FK_FileTaskSessionCache_ActiveFAM]
ALTER TABLE [dbo].[ActiveFAM] DROP CONSTRAINT [FK_ActiveFAM_FAMSession]
ALTER TABLE [Security].[GroupRole] DROP CONSTRAINT [FK_GroupRole_Role_ID]
ALTER TABLE [Security].[GroupRole] DROP CONSTRAINT [FK_GroupRole_Group_ID]
ALTER TABLE [dbo].[LabDEOrderFile] DROP CONSTRAINT [FK_OrderFile_Order]
ALTER TABLE [dbo].[LabDEOrderFile] DROP CONSTRAINT [FK_OrderFile_FAMFile]
ALTER TABLE [dbo].[FAMSession] DROP CONSTRAINT [FK_FAMSession_Action]
ALTER TABLE [dbo].[FAMSession] DROP CONSTRAINT [FK_FAMSession_Machine]
ALTER TABLE [dbo].[FAMSession] DROP CONSTRAINT [FK_FAMSession_User]
ALTER TABLE [dbo].[FAMSession] DROP CONSTRAINT [FK_FAMSession_FPSFile]
ALTER TABLE [dbo].[SecureCounterValueChange] DROP CONSTRAINT [FK_SecureCounterValueChange_FAMSession]
ALTER TABLE [dbo].[SecureCounterValueChange] DROP CONSTRAINT [FK_SecureCounterValueChange_SecureCounter]
ALTER TABLE [dbo].[QueueEvent] DROP CONSTRAINT [FK_QueueEvent_File]
ALTER TABLE [dbo].[QueueEvent] DROP CONSTRAINT [FK_QueueEvent_QueueEventCode]
ALTER TABLE [dbo].[QueueEvent] DROP CONSTRAINT [FK_QueueEvent_Machine]
ALTER TABLE [dbo].[QueueEvent] DROP CONSTRAINT [FK_QueueEvent_FAMUser]
ALTER TABLE [dbo].[QueueEvent] DROP CONSTRAINT [FK_QueueEvent_Action]
ALTER TABLE [dbo].[MLData] DROP CONSTRAINT [FK_MLData_FAMFile]
ALTER TABLE [dbo].[MLData] DROP CONSTRAINT [FK_MLData_MLModel]
ALTER TABLE [dbo].[InputEvent] DROP CONSTRAINT [FK_InputEvent_Action]
ALTER TABLE [dbo].[InputEvent] DROP CONSTRAINT [FK_InputEvent_Machine]
ALTER TABLE [dbo].[InputEvent] DROP CONSTRAINT [FK_InputEvent_User]
ALTER TABLE [dbo].[AttributeInstanceType] DROP CONSTRAINT [FK_Attribute_Instance_Type_AttributeID]
ALTER TABLE [dbo].[AttributeInstanceType] DROP CONSTRAINT [FK_Attribute_Instance_Type_AttributeTypeID]
ALTER TABLE [dbo].[Pagination] DROP CONSTRAINT [FK_Pagination_SourceFile_FAMFile]
ALTER TABLE [dbo].[Pagination] DROP CONSTRAINT [FK_Pagination_DestFile_FAMFile]
ALTER TABLE [dbo].[Pagination] DROP CONSTRAINT [FK_Pagination_OriginalFile_FAMFile]
ALTER TABLE [dbo].[Pagination] DROP CONSTRAINT [FK_Pagination_FileTaskSession]
ALTER TABLE [dbo].[QueuedActionStatusChange] DROP CONSTRAINT [FK_QueuedActionStatusChange_FAMFile]
ALTER TABLE [dbo].[QueuedActionStatusChange] DROP CONSTRAINT [FK_QueuedActionStatusChange_Action]
ALTER TABLE [dbo].[QueuedActionStatusChange] DROP CONSTRAINT [FK_QueuedActionStatusChange_Machine]
ALTER TABLE [dbo].[QueuedActionStatusChange] DROP CONSTRAINT [FK_QueuedActionStatusChange_FAMUser]
ALTER TABLE [dbo].[QueuedActionStatusChange] DROP CONSTRAINT [FK_QueuedActionStatusChange_FAMSession]
ALTER TABLE [dbo].[LockedFile] DROP CONSTRAINT [FK_LockedFile_Action]
ALTER TABLE [dbo].[LockedFile] DROP CONSTRAINT [FK_LockedFile_ActionState]
ALTER TABLE [dbo].[LockedFile] DROP CONSTRAINT [FK_LockedFile_FAMFile]
ALTER TABLE [dbo].[LockedFile] DROP CONSTRAINT [FK_LockedFile_ActiveFAM]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_Action]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_FAMFile]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_Machine]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_FAMUser]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_ActionState_To]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_ActionState_From]
ALTER TABLE [dbo].[FileActionStateTransition] DROP CONSTRAINT [FK_FileActionStateTransition_Queue]
ALTER TABLE [Security].[GroupDashboard] DROP CONSTRAINT [FK_GroupDashboard_Group_ID]
ALTER TABLE [Security].[GroupDashboard] DROP CONSTRAINT [FK_GroupDashboard_Dashboard_Guid]
ALTER TABLE [dbo].[Dashboard] DROP CONSTRAINT [FK_Dashboard_FAMUser]
ALTER TABLE [dbo].[FAMUser] DROP CONSTRAINT [FK_FAMUSER_LOGIN_ID]
ALTER TABLE [dbo].[LabDEEncounter] DROP CONSTRAINT [FK_Encounter_Patient]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Expected]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Found]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMFile]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_DatabaseService]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMUser_Found]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMUser_Expected]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_Action_Found]
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] DROP CONSTRAINT [FK_ReportingDataCaptureAccuracy_Action_Expected]
ALTER TABLE [dbo].[LabDEOrder] DROP CONSTRAINT [FK_Order_Patient]
ALTER TABLE [dbo].[LabDEOrder] DROP CONSTRAINT [FK_Order_OrderStatus]
ALTER TABLE [dbo].[LabDEOrder] DROP CONSTRAINT [FK_Order_Encounter]
ALTER TABLE [dbo].[DocTagHistory] DROP CONSTRAINT [FK_DocTagHistory_FAMFile]
ALTER TABLE [dbo].[DocTagHistory] DROP CONSTRAINT [FK_DocTagHistory_Tag]
ALTER TABLE [dbo].[DocTagHistory] DROP CONSTRAINT [FK_DocTagHistory_FAMUser]
ALTER TABLE [dbo].[DocTagHistory] DROP CONSTRAINT [FK_DocTagHistory_Machine]
ALTER TABLE [dbo].[FileActionComment] DROP CONSTRAINT [FK_FileActionComment_Action]
ALTER TABLE [dbo].[FileActionComment] DROP CONSTRAINT [FK_FileActionComment_FAMFILE]
ALTER TABLE [dbo].[Action] DROP CONSTRAINT [FK_Action_Workflow]
ALTER TABLE [dbo].[FileTaskSession] DROP CONSTRAINT [FK_FileTaskSession_FAMSession]
ALTER TABLE [dbo].[FileTaskSession] DROP CONSTRAINT [FK_FileTaskSession_TaskClass]
ALTER TABLE [dbo].[FileTaskSession] DROP CONSTRAINT [FK_FileTaskSession_FAMFile]
ALTER TABLE [dbo].[RasterZone] DROP CONSTRAINT [FK_RasterZone_AttributeID]
ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_AttributeSetForFileID]
ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_AttributeNameID]
ALTER TABLE [dbo].[Attribute] DROP CONSTRAINT [FK_Attribute_ParentAttributeID]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_Pagination]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_FAMUser]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_FAMFile_SourceFile]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_FAMFile_DestFile]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_FAMFile_OriginalFile]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_FileTaskSession]
ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [FK_ReportingHIMStats_ActionID]
ALTER TABLE [dbo].[FileActionStatus] DROP CONSTRAINT [FK_FileActionStatus_Action]
ALTER TABLE [dbo].[FileActionStatus] DROP CONSTRAINT [FK_FileActionStatus_FAMFile]
ALTER TABLE [dbo].[FileActionStatus] DROP CONSTRAINT [FK_FileActionStatus_ActionStatus]
ALTER TABLE [dbo].[DashboardAttributeFields] DROP CONSTRAINT [FK_DashboardAttributeFields_AttributeSetForFileID]
ALTER TABLE [dbo].[SourceDocChangeHistory] DROP CONSTRAINT [FK_SourceDocChangeHistory_FAMFile]
ALTER TABLE [dbo].[SourceDocChangeHistory] DROP CONSTRAINT [FK_SourceDocChangeHistory_User]
ALTER TABLE [dbo].[SourceDocChangeHistory] DROP CONSTRAINT [FK_SourceDocChangeHistory_Machine]
ALTER TABLE [dbo].[WorkflowFile] DROP CONSTRAINT [FK_WorkflowFile_File]
ALTER TABLE [dbo].[WorkflowFile] DROP CONSTRAINT [FK_WorkflowFile_Workflow]
ALTER TABLE [dbo].[DatabaseService] DROP CONSTRAINT [FK_DatabaseService_Machine]
ALTER TABLE [dbo].[DatabaseService] DROP CONSTRAINT [FK_DatabaseService_ActiveFAM]
ALTER TABLE [dbo].[DatabaseService] DROP CONSTRAINT [FK_DatabaseService_Active_Machine]
ALTER TABLE [dbo].[LabDEPatientFile] DROP CONSTRAINT [FK_PatientFile_PatientMRN]
ALTER TABLE [dbo].[LabDEPatientFile] DROP CONSTRAINT [FK_PatientFile_FAMFile]
ALTER TABLE [dbo].[LabDEPatient] DROP CONSTRAINT [FK_PatientMergedInto]
ALTER TABLE [dbo].[LabDEPatient] DROP CONSTRAINT [FK_PatientCurrentMRN]
ALTER TABLE [dbo].[ReportingVerificationRates] DROP CONSTRAINT [FK_ReportingVerificationRates_FAMFile]
ALTER TABLE [dbo].[ReportingVerificationRates] DROP CONSTRAINT [FK_ReportingVerificationRates_DatabaseService]
ALTER TABLE [dbo].[ReportingVerificationRates] DROP CONSTRAINT [FK_ReportingVerificationRates_Action]
ALTER TABLE [dbo].[ReportingVerificationRates] DROP CONSTRAINT [FK_ReportingVerificationRates_TaskClass]
ALTER TABLE [dbo].[ReportingVerificationRates] DROP CONSTRAINT [FK_ReportingVerificationRates_FileTaskSession]
ALTER TABLE [Security].[GroupWorkflow] DROP CONSTRAINT [FK_GroupWorkflow_Group_ID]
ALTER TABLE [Security].[GroupWorkflow] DROP CONSTRAINT [FK_GroupWorkflow_WORKFLOW_ID]
ALTER TABLE [dbo].[AttributeSetForFile] DROP CONSTRAINT [FK_AttributeSetForFile_FileTaskSessionID]
ALTER TABLE [dbo].[AttributeSetForFile] DROP CONSTRAINT [FK_AttributeSetForFile_AttributeSetNameID]
DELETE FROM [dbo].[DBInfoChangeHistory] WHERE [ID]=1
DELETE FROM [dbo].[DBInfoChangeHistory] WHERE [ID]=2
DELETE FROM [dbo].[DBInfoChangeHistory] WHERE [ID]=3
DELETE FROM [dbo].[DBInfoChangeHistory] WHERE [ID]=4
DELETE FROM [dbo].[DBInfoChangeHistory] WHERE [ID]=5
UPDATE [dbo].[Login] SET [Guid]=N'25dbc3db-5b16-4963-bf65-e7f48b65f2ac' WHERE [ID]=1
UPDATE [dbo].[FAMUser] SET [UserName]=N'William_Parr', [FullUserName]=N'William Parr' WHERE [ID]=1
SET IDENTITY_INSERT [dbo].[AttributeSetName] ON
INSERT INTO [dbo].[AttributeSetName] ([ID], [Description], [Guid]) VALUES (5, N'Test', N'5d712da3-2e27-4a8c-ac9b-b40c85d24833')
INSERT INTO [dbo].[AttributeSetName] ([ID], [Description], [Guid]) VALUES (6, N'Found', N'c8e2bcaa-4915-4f27-9c90-a497f8440bdb')
INSERT INTO [dbo].[AttributeSetName] ([ID], [Description], [Guid]) VALUES (7, N'Expected', N'f61db2c9-be6c-4857-94ad-0d10d0a05a81')
INSERT INTO [dbo].[AttributeSetName] ([ID], [Description], [Guid]) VALUES (8, N'SourceBatchDataFoundByRules', N'bf881a95-efd6-4e62-9468-9368ca042268')
SET IDENTITY_INSERT [dbo].[AttributeSetName] OFF
SET IDENTITY_INSERT [dbo].[Workflow] ON
INSERT INTO [dbo].[Workflow] ([ID], [Name], [WorkflowTypeCode], [Description], [LoadBalanceWeight], [Guid]) VALUES (4, N'Workflow1', N'U', N'', 1, N'62e1a637-177d-460f-b758-adbf2e1ef695')
INSERT INTO [dbo].[Workflow] ([ID], [Name], [WorkflowTypeCode], [Description], [LoadBalanceWeight], [Guid]) VALUES (5, N'BugTesting', N'U', N'', 1, N'925d2105-a0d3-426f-a986-d6b3d3d92578')
INSERT INTO [dbo].[Workflow] ([ID], [Name], [WorkflowTypeCode], [Description], [LoadBalanceWeight], [Guid]) VALUES (6, N'Thoth_HIM', N'E', N'', 1, N'c6af6699-abed-42ac-9a42-d9d4122461ac')
SET IDENTITY_INSERT [dbo].[Workflow] OFF
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 1, 0, '20201223 12:35:23.343')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 2, 0, '20201223 12:35:24.197')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 3, 0, '20201223 12:38:34.233')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 4, 0, '20201223 12:47:47.403')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 5, 0, '20201223 12:47:47.560')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 6, 0, '20201223 12:47:47.723')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 7, 0, '20201223 12:47:47.850')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 8, 0, '20201223 12:47:48.010')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 9, 0, '20201223 12:47:48.193')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 10, 0, '20201223 12:47:48.393')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 11, 0, '20201223 12:55:15.593')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 12, 0, '20201223 12:55:15.950')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 13, 0, '20201223 12:55:16.183')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 14, 0, '20201223 12:55:16.410')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 15, 0, '20201223 12:55:16.707')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 16, 0, '20201223 12:55:17.073')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 17, 0, '20201223 12:55:17.477')
INSERT INTO [dbo].[WorkflowFile] ([WorkflowID], [FileID], [Invisible], [AddedDateTime]) VALUES (4, 18, 0, '20201223 12:55:17.773')
SET IDENTITY_INSERT [dbo].[FAMFile] ON
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (1, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf.tif', 654497, 18, 3, '20201223 12:35:23.340')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (2, N'C:\Demo_HIM\Input\@FullPacket.pdf.tif', 654497, 18, 3, '20201223 12:35:24.193')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (3, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_1.tif', 0, 2, 3, '20201223 12:38:34.223')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (4, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_2.tif', 0, 1, 3, '20201223 12:47:47.383')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (5, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_3.tif', 0, 1, 3, '20201223 12:47:47.557')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (6, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_4.tif', 0, 1, 3, '20201223 12:47:47.720')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (7, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_5.tif', 0, 1, 3, '20201223 12:47:47.847')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (8, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_6.tif', 0, 3, 3, '20201223 12:47:48.007')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (9, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_7.tif', 0, 5, 3, '20201223 12:47:48.193')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (10, N'C:\Demo_HIM\Input\@FullPacket - Copy.pdf_8.tif', 0, 2, 3, '20201223 12:47:48.390')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (11, N'C:\Demo_HIM\Input\@FullPacket.pdf_1.tif', 0, 1, 3, '20201223 12:55:15.580')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (12, N'C:\Demo_HIM\Input\@FullPacket.pdf_2.tif', 0, 1, 3, '20201223 12:55:15.947')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (13, N'C:\Demo_HIM\Input\@FullPacket.pdf_3.tif', 0, 1, 3, '20201223 12:55:16.180')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (14, N'C:\Demo_HIM\Input\@FullPacket.pdf_4.tif', 0, 1, 3, '20201223 12:55:16.407')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (15, N'C:\Demo_HIM\Input\@FullPacket.pdf_5.tif', 0, 3, 3, '20201223 12:55:16.703')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (16, N'C:\Demo_HIM\Input\@FullPacket.pdf_6.tif', 0, 5, 3, '20201223 12:55:17.070')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (17, N'C:\Demo_HIM\Input\@FullPacket.pdf_7.tif', 0, 2, 3, '20201223 12:55:17.473')
INSERT INTO [dbo].[FAMFile] ([ID], [FileName], [FileSize], [Pages], [Priority], [AddedDateTime]) VALUES (18, N'C:\Demo_HIM\Input\@FullPacket.pdf_8.tif', 0, 2, 3, '20201223 12:55:17.770')
SET IDENTITY_INSERT [dbo].[FAMFile] OFF
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'C', 3, 1, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'C', 3, 2, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'C', 3, 11, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 1, 66)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 2, 66)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 3, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 4, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 5, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 6, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 7, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 8, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 9, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 10, 63)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 12, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 13, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 14, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 15, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 16, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 17, 68)
INSERT INTO [dbo].[FileActionStatus] ([ActionStatus], [Priority], [FileID], [ActionID]) VALUES (N'P', 3, 18, 68)
SET IDENTITY_INSERT [dbo].[FileTaskSession] ON
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (1, 1, 68, 2, 1, '20201223 12:39:33.000', 245.956002, 3.2988, 213.601093)
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (2, 2, 68, 2, 1, '20201223 12:47:49.203', 480.17841, 5.086975, 118.01569)
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (3, 2, 68, 2, 2, '20201223 12:47:59.013', 9.455354, 1.431546, 0)
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (4, 3, 68, 9, 2, '20201223 12:55:18.663', 274.91, 0, 42.21332)
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (5, 3, 68, 9, 11, '20201223 12:55:44.533', 25.821, 0.048, 25.821812)
INSERT INTO [dbo].[FileTaskSession] ([ID], [FAMSessionID], [ActionID], [TaskClassID], [FileID], [DateTimeStamp], [Duration], [OverheadTime], [ActivityTime]) VALUES (6, 3, 68, 9, 12, '20201223 12:55:50.183', 5.614, 0.035, 5.614333)
SET IDENTITY_INSERT [dbo].[FileTaskSession] OFF
SET IDENTITY_INSERT [dbo].[Action] ON
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (40, N'A30_ReRunRules', NULL, NULL, NULL, N'44ef605a-d679-460f-ae7b-a0ce43eb9fc7')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (41, N'A40_Apply_Watermarks', NULL, NULL, NULL, N'e8da69cd-2149-46b0-8182-3065c4430665')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (42, N'A80_Cleanup', NULL, NULL, NULL, N'a3351668-5dcb-4a71-b609-2df39d004090')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (43, N'CreateDups', NULL, NULL, NULL, N'db93f6c9-e3d1-4e22-8ccf-d4dad08557e5')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (44, N'Verify', NULL, NULL, NULL, N'b0a05b00-9162-4d66-acca-544214149cda')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (45, N'Z00_Rasterize', NULL, NULL, NULL, N'2f4c825a-ebf1-401d-946b-315361863d30')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (46, N'Z01_Run_Rules', NULL, NULL, NULL, N'c91143d2-0b83-42a8-8b13-79acbc2ba1ba')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (47, N'Z02_SearchEF', NULL, NULL, NULL, N'8f227da4-d08d-40e8-83e1-9917d15d9cbc')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (48, N'z1', NULL, NULL, NULL, N'db687ff9-0a25-4b92-ae79-8a9d0412844c')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (49, N'z2', NULL, NULL, NULL, N'10370294-0e59-4f23-bc92-9ce0a9ae8a2d')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (50, N'z3', NULL, NULL, NULL, N'fc674119-e06e-48b3-a35c-5e284ceb24c1')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (51, N'z4a', NULL, NULL, NULL, N'50581e59-cd7c-459f-a935-1c7d970a275d')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (52, N'CreatePaginatedOutput', NULL, NULL, NULL, N'1ea5ddd9-d04e-498f-a876-60553f5bbe7b')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (53, N'Z02_AutoPaginate', NULL, NULL, NULL, N'164938bf-b11c-4d9a-be5d-b23f7a581ec7')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (54, N'Z03_PaginatedOutput', NULL, NULL, NULL, N'd071ea41-6d67-42a1-a1b0-738418e1594c')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (55, N'Z04_PartiallyPaginatedSources', NULL, NULL, NULL, N'c3677f82-4a64-45d1-9d66-abc3eed6069d')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (56, N'Z05_FullyPaginated', NULL, NULL, NULL, N'74b39897-d638-4d66-81a9-1b7f3ddc0022')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (57, N'zAdmin', NULL, NULL, NULL, N'4ceefce7-f880-4f0a-b8a9-f56eb6bc0278')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (58, N'zAdmin', NULL, 4, 1, N'59fd323d-f37c-4bf4-ab97-b6cd5d74fd08')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (59, N'Z05_FullyPaginated', NULL, 4, 1, N'e3daf4ec-92b4-4106-b6d7-8b337d82742c')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (60, N'Z04_PartiallyPaginatedSources', NULL, 4, 1, N'36dbafa1-0426-4f7d-955a-64d04ab09666')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (61, N'Z03_PaginatedOutput', NULL, 4, 1, N'd0bfd3da-7b40-4222-b7ef-5a8aed3c9c53')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (62, N'Z02_AutoPaginate', NULL, 4, 1, N'4ef3ab50-2c36-4346-a622-11f76bf68f08')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (63, N'CreatePaginatedOutput', NULL, 4, 1, N'49c7bc7c-5be5-4d2a-ad32-9e04beda2cf4')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (64, N'A30_ReRunRules', NULL, 4, 1, N'1d9627a6-4971-42cd-a171-0d94c4ae989a')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (65, N'A40_Apply_Watermarks', NULL, 4, 1, N'3e71335a-79cd-4fa3-a8cf-d73ae5870666')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (66, N'A80_Cleanup', NULL, 4, 1, N'3bab183e-512c-48dc-97a9-ba8b6634985f')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (67, N'CreateDups', NULL, 4, 1, N'bbd1dc85-dc4b-4c1a-9615-e88e13f4cb35')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (68, N'Verify', NULL, 4, 1, N'62af3816-5315-4a4e-89da-62c5ebd4e82a')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (69, N'Z00_Rasterize', NULL, 4, 1, N'c23efede-5f5f-4c9c-8db2-bb7d4fa51d49')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (70, N'Z01_Run_Rules', NULL, 4, 1, N'a882e24f-0978-4de4-90b6-fa9ac714729d')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (71, N'Z02_SearchEF', NULL, 4, 1, N'9c20ae10-3078-431b-bff7-9182108364fd')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (72, N'z1', NULL, 4, 1, N'2b8f0181-b482-4530-914a-2a9fdde70019')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (73, N'z2', NULL, 4, 1, N'30299553-aca8-4848-b64f-a13557c7c15d')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (74, N'z3', NULL, 4, 1, N'04190153-64fd-4363-b8f7-121358b58cf1')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (75, N'z1', NULL, 5, 1, N'f395b7e6-68e3-4aa0-8f20-0a4dcb66ca4a')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (76, N'Verify', NULL, 6, 1, N'4616e332-e38f-481e-9822-3b28441f15e6')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (77, N'A80_Cleanup', NULL, 6, 1, N'dec71ff6-ad76-4fa3-931d-81e689f48ca0')
INSERT INTO [dbo].[Action] ([ID], [ASCName], [Description], [WorkflowID], [MainSequence], [Guid]) VALUES (78, N'z1', NULL, 6, 1, N'86608ca3-9575-4828-83dd-2fc65a483986')
SET IDENTITY_INSERT [dbo].[Action] OFF
SET IDENTITY_INSERT [dbo].[FAMUser] ON
INSERT INTO [dbo].[FAMUser] ([ID], [UserName], [FullUserName], [LoginID]) VALUES (2, N'la_william_parr', N'William Parr Local Admin', NULL)
SET IDENTITY_INSERT [dbo].[FAMUser] OFF
SET IDENTITY_INSERT [dbo].[FileActionStateTransition] ON
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (1, 1, 68, N'P', N'R', '20201223 12:35:26.847', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (2, 3, 63, N'U', N'P', '20201223 12:38:34.740', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (3, 1, 68, N'R', N'P', '20201223 12:39:32.843', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (4, 1, 68, N'P', N'R', '20201223 12:39:48.937', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (5, 4, 63, N'U', N'P', '20201223 12:47:47.513', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (6, 5, 63, N'U', N'P', '20201223 12:47:47.663', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (7, 6, 63, N'U', N'P', '20201223 12:47:47.803', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (8, 7, 63, N'U', N'P', '20201223 12:47:47.957', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (9, 8, 63, N'U', N'P', '20201223 12:47:48.143', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (10, 9, 63, N'U', N'P', '20201223 12:47:48.343', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (11, 10, 63, N'U', N'P', '20201223 12:47:48.477', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (12, 1, 68, N'R', N'C', '20201223 12:47:49.220', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (13, 1, 66, N'U', N'P', '20201223 12:47:49.233', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (14, 2, 68, N'P', N'R', '20201223 12:47:49.460', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (15, 2, 68, N'R', N'P', '20201223 12:47:58.893', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (16, 2, 68, N'P', N'R', '20201223 12:50:43.673', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (17, 11, 68, N'U', N'P', '20201223 12:55:15.857', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (18, 11, 68, N'P', N'R', '20201223 12:55:15.857', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (19, 12, 68, N'U', N'P', '20201223 12:55:16.110', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (20, 12, 68, N'P', N'R', '20201223 12:55:16.110', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (21, 13, 68, N'U', N'P', '20201223 12:55:16.347', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (22, 13, 68, N'P', N'R', '20201223 12:55:16.347', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (23, 14, 68, N'U', N'P', '20201223 12:55:16.610', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (24, 14, 68, N'P', N'R', '20201223 12:55:16.610', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (25, 15, 68, N'U', N'P', '20201223 12:55:16.943', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (26, 15, 68, N'P', N'R', '20201223 12:55:16.943', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (27, 16, 68, N'U', N'P', '20201223 12:55:17.367', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (28, 16, 68, N'P', N'R', '20201223 12:55:17.367', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (29, 17, 68, N'U', N'P', '20201223 12:55:17.660', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (30, 17, 68, N'P', N'R', '20201223 12:55:17.660', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (31, 18, 68, N'U', N'P', '20201223 12:55:17.983', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (32, 18, 68, N'P', N'R', '20201223 12:55:17.983', 1, 1, N'', N'', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (33, 2, 66, N'U', N'P', '20201223 12:55:18.023', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (34, 2, 68, N'R', N'C', '20201223 12:55:18.677', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (35, 11, 68, N'R', N'C', '20201223 12:55:44.543', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (36, 12, 68, N'R', N'P', '20201223 12:55:50.333', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (37, 13, 68, N'R', N'P', '20201223 12:55:50.417', 1, 1, NULL, NULL, NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (38, 14, 68, N'R', N'P', '20201223 12:55:50.470', 1, 1, NULL, N'Processing FAM is exiting.', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (39, 15, 68, N'R', N'P', '20201223 12:55:50.483', 1, 1, NULL, N'Processing FAM is exiting.', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (40, 16, 68, N'R', N'P', '20201223 12:55:50.493', 1, 1, NULL, N'Processing FAM is exiting.', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (41, 17, 68, N'R', N'P', '20201223 12:55:50.503', 1, 1, NULL, N'Processing FAM is exiting.', NULL)
INSERT INTO [dbo].[FileActionStateTransition] ([ID], [FileID], [ActionID], [ASC_From], [ASC_To], [DateTimeStamp], [MachineID], [FAMUserID], [Exception], [Comment], [QueueID]) VALUES (42, 18, 68, N'R', N'P', '20201223 12:55:50.513', 1, 1, NULL, N'Processing FAM is exiting.', NULL)
SET IDENTITY_INSERT [dbo].[FileActionStateTransition] OFF
SET IDENTITY_INSERT [dbo].[Pagination] ON
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (1, 1, 17, 3, 1, 1, 17, 1)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (2, 1, 18, 3, 2, 1, 18, 1)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (3, 1, 1, NULL, NULL, 1, 1, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (4, 1, 2, 4, 1, 1, 2, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (5, 1, 3, 5, 1, 1, 3, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (6, 1, 4, 6, 1, 1, 4, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (7, 1, 5, 7, 1, 1, 5, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (8, 1, 6, 8, 1, 1, 6, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (9, 1, 7, 8, 2, 1, 7, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (10, 1, 8, 8, 3, 1, 8, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (11, 1, 9, 8, NULL, 1, 9, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (12, 1, 10, 9, 1, 1, 10, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (13, 1, 11, 9, 2, 1, 11, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (14, 1, 12, 9, 3, 1, 12, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (15, 1, 13, 9, 4, 1, 13, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (16, 1, 14, 9, 5, 1, 14, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (17, 1, 15, 10, 1, 1, 15, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (18, 1, 16, 10, 2, 1, 16, 2)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (19, 2, 1, NULL, NULL, 2, 1, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (20, 2, 2, 11, 1, 2, 2, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (21, 2, 2, 11, 1, 2, 2, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (22, 2, 3, 12, 1, 2, 3, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (23, 2, 3, 12, 1, 2, 3, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (24, 2, 4, 13, 1, 2, 4, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (25, 2, 4, 13, 1, 2, 4, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (26, 2, 5, 14, 1, 2, 5, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (27, 2, 5, 14, 1, 2, 5, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (28, 2, 6, 15, 1, 2, 6, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (29, 2, 7, 15, 2, 2, 7, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (30, 2, 8, 15, 3, 2, 8, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (31, 2, 9, 15, NULL, 2, 9, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (32, 2, 6, 15, 1, 2, 6, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (33, 2, 7, 15, 2, 2, 7, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (34, 2, 8, 15, 3, 2, 8, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (35, 2, 9, 15, NULL, 2, 9, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (36, 2, 10, 16, 1, 2, 10, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (37, 2, 11, 16, 2, 2, 11, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (38, 2, 12, 16, 3, 2, 12, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (39, 2, 13, 16, 4, 2, 13, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (40, 2, 14, 16, 5, 2, 14, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (41, 2, 10, 16, 1, 2, 10, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (42, 2, 11, 16, 2, 2, 11, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (43, 2, 12, 16, 3, 2, 12, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (44, 2, 13, 16, 4, 2, 13, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (45, 2, 14, 16, 5, 2, 14, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (46, 2, 15, 17, 1, 2, 15, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (47, 2, 16, 17, 2, 2, 16, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (48, 2, 15, 17, 1, 2, 15, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (49, 2, 16, 17, 2, 2, 16, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (50, 2, 17, 18, 1, 2, 17, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (51, 2, 18, 18, 2, 2, 18, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (52, 2, 17, 18, 1, 2, 17, 4)
INSERT INTO [dbo].[Pagination] ([ID], [SourceFileID], [SourcePage], [DestFileID], [DestPage], [OriginalFileID], [OriginalPage], [FileTaskSessionID]) VALUES (53, 2, 18, 18, 2, 2, 18, 4)
SET IDENTITY_INSERT [dbo].[Pagination] OFF
SET IDENTITY_INSERT [dbo].[InputEvent] ON
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (1, '20201223 12:35:00.000', 68, 1, 1, 42252, 6)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (2, '20201223 12:36:00.000', 68, 1, 1, 42252, 25)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (3, '20201223 12:37:00.000', 68, 1, 1, 42252, 35)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (4, '20201223 12:38:00.000', 68, 1, 1, 42252, 23)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (5, '20201223 12:39:00.000', 68, 1, 1, 42252, 6)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (6, '20201223 12:40:00.000', 68, 1, 1, 42252, 20)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (7, '20201223 12:41:00.000', 68, 1, 1, 42252, 4)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (8, '20201223 12:42:00.000', 68, 1, 1, 42252, 1)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (9, '20201223 12:46:00.000', 68, 1, 1, 42252, 11)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (10, '20201223 12:47:00.000', 68, 1, 1, 42252, 26)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (11, '20201223 12:50:00.000', 68, 1, 1, 42252, 7)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (12, '20201223 12:51:00.000', 68, 1, 1, 42252, 7)
INSERT INTO [dbo].[InputEvent] ([ID], [TimeStamp], [ActionID], [FAMUserID], [MachineID], [PID], [SecondsWithInputEvents]) VALUES (13, '20201223 12:55:00.000', 68, 1, 1, 42252, 13)
SET IDENTITY_INSERT [dbo].[InputEvent] OFF
SET IDENTITY_INSERT [dbo].[QueueEvent] ON
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (1, 1, 68, '20201223 12:35:23.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (2, 2, 68, '20201223 12:35:24.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (3, 3, NULL, '20201223 12:38:34.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (4, 1, 68, '20201223 12:39:48.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (5, 2, 68, '20201223 12:39:48.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (6, 4, NULL, '20201223 12:47:47.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (7, 5, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (8, 6, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (9, 7, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (10, 8, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (11, 9, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (12, 10, NULL, '20201223 12:47:48.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (13, 1, 68, '20201223 12:50:42.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (14, 2, 68, '20201223 12:50:42.000', N'A', '20170504 17:16:28.000', 654497, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (15, 11, NULL, '20201223 12:55:16.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (16, 12, NULL, '20201223 12:55:16.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (17, 13, NULL, '20201223 12:55:16.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (18, 14, NULL, '20201223 12:55:16.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (19, 15, NULL, '20201223 12:55:17.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (20, 16, NULL, '20201223 12:55:17.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (21, 17, NULL, '20201223 12:55:17.000', N'P', NULL, 0, 1, 1)
INSERT INTO [dbo].[QueueEvent] ([ID], [FileID], [ActionID], [DateTimeStamp], [QueueEventCode], [FileModifyTime], [FileSizeInBytes], [MachineID], [FAMUserID]) VALUES (22, 18, NULL, '20201223 12:55:18.000', N'P', NULL, 0, 1, 1)
SET IDENTITY_INSERT [dbo].[QueueEvent] OFF
SET IDENTITY_INSERT [dbo].[FAMSession] ON
INSERT INTO [dbo].[FAMSession] ([ID], [MachineID], [FAMUserID], [UPI], [StartTime], [StopTime], [FPSFileID], [ActionID], [Queuing], [Processing]) VALUES (1, 1, 1, N'VOYAGER\ProcessFiles\42252\12232020\12:35:18', '20201223 12:35:23.220', '20201223 12:39:33.110', 1, 68, 1, 1)
INSERT INTO [dbo].[FAMSession] ([ID], [MachineID], [FAMUserID], [UPI], [StartTime], [StopTime], [FPSFileID], [ActionID], [Queuing], [Processing]) VALUES (2, 1, 1, N'VOYAGER\ProcessFiles\42252\12232020\12:35:18', '20201223 12:39:47.403', '20201223 12:47:59.110', 1, 68, 1, 1)
INSERT INTO [dbo].[FAMSession] ([ID], [MachineID], [FAMUserID], [UPI], [StartTime], [StopTime], [FPSFileID], [ActionID], [Queuing], [Processing]) VALUES (3, 1, 1, N'VOYAGER\ProcessFiles\42252\12232020\12:35:18', '20201223 12:50:41.397', '20201223 12:55:50.630', 1, 68, 1, 1)
INSERT INTO [dbo].[FAMSession] ([ID], [MachineID], [FAMUserID], [UPI], [StartTime], [StopTime], [FPSFileID], [ActionID], [Queuing], [Processing]) VALUES (1002, 1, 2, N'VOYAGER\FAMDBAdmin\107892\08282021\13:56:41', '20210828 14:22:50.103', '20210828 14:22:51.840', 2, NULL, 0, 0)
SET IDENTITY_INSERT [dbo].[FAMSession] OFF
SET IDENTITY_INSERT [dbo].[MetadataField] ON
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (10, N'PatientFirstName', N'47bfea48-d6b6-4876-865f-416f5ca4a3cb')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (11, N'PatientLastName', N'0bafcec5-d2db-4924-adca-d6054975a38f')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (12, N'PatientDOB', N'0970fc44-5162-4d7a-ad5e-147fc1044107')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (13, N'DocumentDate', N'9bbc7d6d-1914-4fb2-bf47-1971af929166')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (14, N'DocumentType', N'9bcca374-adc7-4dd3-84c2-1cc25714b96b')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (15, N'DocDateAndType', N'e3cbc687-d177-4e43-bb98-99fe99399984')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (16, N'CollectionDate', N'48c3913c-a183-4afe-8fff-461cd7ff2fd2')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (17, N'StapledInto', N'24f4675e-27ab-40b4-97d3-bfe45cce5bac')
INSERT INTO [dbo].[MetadataField] ([ID], [Name], [Guid]) VALUES (18, N'SourceBatchFilename', N'accf00f9-4c72-40f9-bd74-478e85a911bc')
SET IDENTITY_INSERT [dbo].[MetadataField] OFF
SET IDENTITY_INSERT [dbo].[Tag] ON
INSERT INTO [dbo].[Tag] ([ID], [TagName], [TagDescription], [Guid]) VALUES (3, N'Ignore', N'', N'7be08232-acac-4424-b0d7-751f11afb655')
INSERT INTO [dbo].[Tag] ([ID], [TagName], [TagDescription], [Guid]) VALUES (4, N'AutoPaginated', N'', N'408e51f7-d1ec-44f6-b06d-0b0573b0ddba')
SET IDENTITY_INSERT [dbo].[Tag] OFF
SET IDENTITY_INSERT [dbo].[FileHandler] ON
INSERT INTO [dbo].[FileHandler] ([ID], [Enabled], [AppName], [IconPath], [ApplicationPath], [Arguments], [AdminOnly], [AllowMultipleFiles], [SupportsErrorHandling], [Blocking], [WorkflowName], [Guid]) VALUES (1, 1, N'Verify Now - Akpinar', NULL, N'C:\Engineering\binaries\debug\RunFPSFile.exe', N'"C:\Demo_HIM\FPS\HurleyDEPs.fps" "<SourceDocName>" /process', 0, 0, 1, 1, N'Workflow1', N'7cb48ab7-bdb8-442c-a9e9-c53c2ff6b507')
SET IDENTITY_INSERT [dbo].[FileHandler] OFF
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (40, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (40, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (41, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (41, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (42, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (42, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (43, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (43, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (44, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (44, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (45, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (45, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (46, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (46, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (47, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (47, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (48, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (48, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (49, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (49, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (50, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (50, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (51, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (51, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (52, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (52, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (53, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (53, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (54, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (54, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (55, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (55, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (56, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (56, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (57, 0, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (57, 1, '20210828 13:57:18.457', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (58, 0, '20210828 13:57:45.573', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (58, 1, '20210828 13:57:45.573', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (59, 0, '20210828 13:57:45.063', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (59, 1, '20210828 13:57:45.063', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (60, 0, '20210828 13:57:44.677', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (60, 1, '20210828 13:57:44.677', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (61, 0, '20210828 13:57:44.073', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (61, 1, '20210828 13:57:44.073', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (62, 0, '20210828 13:57:43.440', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (62, 1, '20210828 13:57:43.440', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (63, 0, '20210828 13:57:42.513', 8, 8, 0, 0, 0, 16, 16, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (63, 1, '20210828 13:57:42.513', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (64, 0, '20210828 13:57:42.273', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (64, 1, '20210828 13:57:42.273', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (65, 0, '20210828 13:57:42.307', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (65, 1, '20210828 13:57:42.307', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (66, 0, '20210828 13:57:42.433', 2, 2, 0, 0, 0, 36, 36, 0, 0, 0, 1308994, 1308994, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (66, 1, '20210828 13:57:42.433', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (67, 0, '20210828 13:57:42.477', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (67, 1, '20210828 13:57:42.477', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (68, 0, '20210828 13:57:42.560', 10, 7, 3, 0, 0, 52, 15, 37, 0, 0, 1308994, 0, 1308994, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (68, 1, '20210828 13:57:42.560', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (69, 0, '20210828 13:57:42.620', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (69, 1, '20210828 13:57:42.620', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (70, 0, '20210828 13:57:43.143', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (70, 1, '20210828 13:57:43.143', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (71, 0, '20210828 13:57:43.470', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (71, 1, '20210828 13:57:43.470', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (72, 0, '20210828 13:57:45.090', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (72, 1, '20210828 13:57:45.090', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (73, 0, '20210828 13:57:45.110', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (73, 1, '20210828 13:57:45.110', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (74, 0, '20210828 13:57:45.137', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (74, 1, '20210828 13:57:45.137', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (75, 0, '20210828 13:57:35.573', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (75, 1, '20210828 13:57:35.573', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (76, 0, '20210828 13:57:38.867', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (76, 1, '20210828 13:57:38.867', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (77, 0, '20210828 13:57:38.520', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (77, 1, '20210828 13:57:38.520', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (78, 0, '20210828 13:57:39.060', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
INSERT INTO [dbo].[ActionStatistics] ([ActionID], [Invisible], [LastUpdateTimeStamp], [NumDocuments], [NumDocumentsPending], [NumDocumentsComplete], [NumDocumentsFailed], [NumDocumentsSkipped], [NumPages], [NumPagesPending], [NumPagesComplete], [NumPagesFailed], [NumPagesSkipped], [NumBytes], [NumBytesPending], [NumBytesComplete], [NumBytesFailed], [NumBytesSkipped]) VALUES (78, 1, '20210828 13:57:39.060', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
SET IDENTITY_INSERT [dbo].[FPSFile] ON
INSERT INTO [dbo].[FPSFile] ([ID], [FPSFileName]) VALUES (1, N'C:\PaginationTest\HurleyDEPs.fps')
INSERT INTO [dbo].[FPSFile] ([ID], [FPSFileName]) VALUES (2, N'<Admin>')
SET IDENTITY_INSERT [dbo].[FPSFile] OFF
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'01f1b21d-a1ac-4b2e-be3e-06810c89c3f8', N'Info', N'FAMUser', N'The FAMUser SYSTEM will be added to the database', '20201223 12:15:22.410', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'0240f495-09e3-47c7-8272-378a60efc4ee', N'Warning', N'DatabaseService', N'The service Expand Attributes will be added to the database. Please be sure the configuration/schedule is appropriate for the new enviornment.', '20201223 12:15:22.537', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'026e0e94-f2ac-43d8-8d71-bea68a31894e', N'Info', N'AttributeSetName', N'The Attribute Set Name Found will be added to the database', '20201223 12:15:21.877', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'0b8d5352-547d-4084-b11a-94237da307bf', N'Info', N'MetadataField', N'The MetadataField PatientLastName will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'0f922366-391c-4672-8e2c-9921f8926c8d', N'Info', N'Action', N'The action Z00_Rasterize will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'1057dbec-ab16-47b8-8110-11bde6a1c59d', N'Info', N'LabDEEncounter', N'The LabDEEncounter table will have 0 rows updated.', '20201223 12:15:22.613', NULL, NULL, N'Update')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'12aff020-f317-43ca-9f80-9cd9979d25e8', N'Info', N'Action', N'The action Z01_Run_Rules will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'16b14d50-502c-400f-9074-4e7ac4a3fef6', N'Info', N'Workflow', N'The workflow Workflow1 will be added to the database', '20201223 12:15:22.353', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'17e1ed3a-a817-463f-82ef-d082a795a2c6', N'Info', N'Action', N'The action z1 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'196d980b-afd4-44cb-8193-6fad379cdb69', N'Info', N'Action', N'The action zAdmin will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'1a60ecde-7132-4c05-99ce-7e1eb6ef472c', N'Info', N'Tag', N'The Tag Ignore will be added to the database', '20201223 12:15:22.327', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'1e4ba8fb-69ab-4536-bb5c-24d463a4dfd0', N'Warning', N'DBInfo', N'The DBInfo UseGetFilesLegacy is present in the destination database, but NOT in the importing source.', '20201223 12:15:23.913', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'1fb27803-22db-4f01-b0fa-c1821d649b26', N'Info', N'Action', N'The action Verify will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'293aa480-0258-4607-a163-70da51a0c551', N'Info', N'Action', N'The action Z02_SearchEF will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'2db2d99c-4a89-4daf-af43-86b1bf3c8bb0', N'Info', N'Action', N'The action A80_Cleanup will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'30b593a8-fa17-4b1b-8d3d-ee48e0802140', N'Info', N'FAMUser', N'The FAMUser steve_kurth will be added to the database', '20201223 12:15:22.410', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'3178a9e4-b80d-45ad-bd0a-cc3455ede498', N'Info', N'Workflow', N'The workflow Thoth_HIM will be added to the database', '20201223 12:15:22.353', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'39f717f1-8602-4e96-afaa-0439e17926d6', N'Info', N'Action', N'The action Verify will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'3accbfed-feb7-495e-8995-77c31b9d119f', N'Info', N'AttributeSetName', N'The Attribute Set Name SourceBatchDataFoundByRules will be added to the database', '20201223 12:15:21.877', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'3b527658-bfa9-479c-8eff-7f0cda80391b', N'Info', N'Action', N'The action z2 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'3d039fdb-3715-4b86-bf93-a5e877f342c0', N'Info', N'Action', N'The action z1 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'3fea3177-b1d1-4424-8c7c-1a6cea1ae56a', N'Info', N'LabDEOrder', N'The LabDEOrder table will have 0 rows updated.', '20201223 12:15:22.717', NULL, NULL, N'Update')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'40139081-a640-4dea-b571-0e5e7a34f7ca', N'Warning', N'DBInfo', N'The SkipAuthenticationForServiceOnMachines will have its value updated', '20201223 12:15:23.913', N'*', N'hawkeye;colby', N'Update')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'4721f6af-3017-4496-add7-b6eabf5a2a50', N'Info', N'Action', N'The action Z02_AutoPaginate will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'488f2b72-d1ce-4b95-adf9-37a0cdb6be88', N'Info', N'Action', N'The action Z00_Rasterize will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'4b95d407-8c70-4362-94de-776d3364ac63', N'Info', N'Tag', N'The Tag AutoPaginated will be added to the database', '20201223 12:15:22.327', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'52437b45-a62d-46d2-bff4-9a41a0f8d55e', N'Info', N'Action', N'The action Z05_FullyPaginated will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'534f129e-a9f3-465a-88f8-a984680284ff', N'Info', N'AttributeSetName', N'The Attribute Set Name Test will be added to the database', '20201223 12:15:21.877', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'5421f887-ebc9-4bc7-9897-8f2174143a61', N'Info', N'AttributeSetName', N'The Attribute Set Name Expected will be added to the database', '20201223 12:15:21.877', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'5c0458b0-0e07-4ea5-9786-443ddb7ba025', N'Info', N'Action', N'The action CreatePaginatedOutput will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'629961f6-2441-4e15-a614-7fec8b26abc2', N'Info', N'LabDEOrder', N'The LabDEPatient table will have 0 rows updated.', '20201223 12:15:22.473', NULL, NULL, N'Update')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'6ac5cd47-e750-466e-83b6-135e67579bd4', N'Info', N'Action', N'The action A40_Apply_Watermarks will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'6b1267f0-ccb7-4917-9ff8-2539ec14a570', N'Info', N'MetadataField', N'The MetadataField DocumentType will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'70acad45-b046-4b5c-a587-02654ed44859', N'Info', N'LabDEOrder', N'The LabDEOrder table will have 87 rows added to the database', '20201223 12:15:22.703', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'7548cc20-8909-48e5-9464-d50467d43dd5', N'Info', N'LabDEProvider', N'The LabDEProvider table will have 0 rows added to the database', '20201223 12:15:22.513', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'75a01f19-d188-4a87-87e4-efe844454888', N'Info', N'MetadataField', N'The MetadataField DocDateAndType will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'780d9781-eaa9-4a15-af67-55996b10f7a1', N'Info', N'Action', N'The action A80_Cleanup will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'7a443af7-4f43-4480-8a79-26ddfddee8e6', N'Info', N'Action', N'The action Z04_PartiallyPaginatedSources will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'7a9056e9-f43d-461e-99ce-367780af6b76', N'Info', N'MetadataField', N'The MetadataField SourceBatchFilename will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'7ea16d3b-80cd-4fab-b5a0-af9a95f65ec7', N'Info', N'Action', N'The action z3 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'7ff37bd5-081c-41e1-8006-eb42215db29a', N'Info', N'Action', N'The action Z03_PaginatedOutput will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8066d3f9-3b99-4fe3-b60c-74b412556de1', N'Info', N'Action', N'The action Z02_SearchEF will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'81a83bcf-b717-4616-8fd3-a61e0ba80d52', N'Info', N'Action', N'The action A40_Apply_Watermarks will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'85c86c4f-9813-46a6-973e-47f5c3c88851', N'Info', N'FAMUser', N'The FAMUser EXTRACT\steve_kurth will be added to the database', '20201223 12:15:22.410', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'87a021b8-33df-46bb-9a3b-ee67992ef8cc', N'Info', N'MetadataField', N'The MetadataField CollectionDate will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8958431f-711e-4f04-a909-4d1babee2a7b', N'Warning', N'DBInfo', N'The DBInfo DashboardExcludeFilter is present in the destination database, but NOT in the importing source.', '20201223 12:15:23.913', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8ac1ecfd-ff1c-4086-9f22-b640e2a63e9c', N'Warning', N'FAMUser', N'The FAMUser William_Parr is present in the destination database, but NOT in the importing source.', '20201223 12:15:22.410', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8c902311-7a54-479a-a6fd-1f179990901b', N'Info', N'Action', N'The action Verify will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8cf3f353-b409-4c5c-afeb-9fcd74e42337', N'Info', N'LabDEPatient', N'The LabDEPatient table will have 15 rows added to the database', '20201223 12:15:22.463', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8f0c1f09-2edd-48f8-893c-48021f84e59e', N'Info', N'Action', N'The action CreatePaginatedOutput will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'8fe391d9-4a39-4a05-8ebb-28a617f1301f', N'Info', N'Action', N'The action CreateDups will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'92b01cbd-4767-41bc-8b16-356ee4948c8b', N'Info', N'Action', N'The action zAdmin will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'99cc998d-68c4-4c0f-b8f3-2a24d8ec5007', N'Info', N'Action', N'The action z1 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'9e922b7b-d8b7-4b2b-b5f6-8d7e67bb23d5', N'Info', N'Action', N'The action Z05_FullyPaginated will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'a89d5630-87f7-41e2-9dca-a38d492eba8a', N'Info', N'Action', N'The action A80_Cleanup will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'b0133ffe-8276-4a41-964b-60385d732489', N'Info', N'Action', N'The action Z02_AutoPaginate will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'b2b3dcf0-1fb6-434a-a71b-f50b7966fd74', N'Info', N'Action', N'The action z2 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'b5ab5d5c-7c38-457b-8568-f5e91647693b', N'Info', N'Action', N'The action A30_ReRunRules will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'b9b1b20d-be3b-48d5-a1ee-1ae048b8e3e8', N'Warning', N'DBInfo', N'The RequirePasswordToProcessAllSkippedFiles will have its value updated', '20201223 12:15:23.913', N'1', N'0', N'Update')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'bb1f7d4d-8f45-48aa-9c84-3e68bc9a7391', N'Warning', N'DatabaseService', N'The service DataCaptureAccuracy will be added to the database. Please be sure the configuration/schedule is appropriate for the new enviornment.', '20201223 12:15:22.537', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'bc53b708-ea1c-44ac-a567-4afb4894abaa', N'Info', N'MetadataField', N'The MetadataField PatientDOB will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'be1e7047-a33d-4704-b352-7179cb81efcf', N'Info', N'Action', N'The action Z01_Run_Rules will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'bf216a3c-f4d5-416c-811d-f4e5bf1eb03b', N'Info', N'Action', N'The action z3 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'bf34efa7-3a41-4415-8379-8fa29cee53d3', N'Info', N'Action', N'The action Z04_PartiallyPaginatedSources will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'c67832b3-2240-4f70-acab-86cb5a41cc33', N'Info', N'Action', N'The action A30_ReRunRules will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'cfb51b5a-22b2-46fe-8bb6-6c1bf45a28c0', N'Info', N'LabDEEncounter', N'The LabDEEncounter table will have 30 rows added to the database', '20201223 12:15:22.603', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'd22f6d84-4360-466a-be68-b971edf58528', N'Info', N'Action', N'The action Z03_PaginatedOutput will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'dde6c811-3a23-425c-93ec-b345c2f09601', N'Info', N'MetadataField', N'The MetadataField PatientFirstName will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'ded9d1d8-ef5f-4794-b2f7-bd7cf1efec33', N'Warning', N'DBInfo', N'Uninitialized setting! RootPathForDashboardExtractedData will not be set in the destination', '20201223 12:15:23.913', NULL, NULL, N'N/A')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'e0be5050-54bb-42ce-a8ff-3130581cb7d7', N'Info', N'Action', N'The action z4a will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'e1be2365-3840-4e98-90ae-b1b1f8cf46c0', N'Info', N'FileHandler', N'The FileHandler Verify Now - Akpinar will be added to the database', '20201223 12:15:22.817', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'e54e0ea0-81b5-49d5-8e7a-5531eea608f2', N'Info', N'Workflow', N'The workflow BugTesting will be added to the database', '20201223 12:15:22.353', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'e8787456-6b7c-4472-a7ea-e651db1b2142', N'Info', N'MetadataField', N'The MetadataField DocumentDate will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'ee160e37-904e-4313-a683-eb721c11a4c8', N'Warning', N'DBInfo', N'The DBInfo DashboardIncludeFilter is present in the destination database, but NOT in the importing source.', '20201223 12:15:23.913', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'f32ff9e3-0b00-4f03-915b-e42942fa60e1', N'Info', N'Action', N'The action z1 will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'f5e6ea1b-6425-443f-9b84-856e24f94257', N'Info', N'Action', N'The action CreateDups will be added to the database', '20201223 12:15:22.427', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'fbe2b443-825a-49ae-83ee-059b37c4b8d4', N'Info', N'MetadataField', N'The MetadataField StapledInto will be added to the database', '20201223 12:15:22.313', NULL, NULL, N'Insert')
INSERT INTO [dbo].[ReportingDatabaseMigrationWizard] ([ID], [Classification], [TableName], [Message], [DateTime], [Old_Value], [New_Value], [Command]) VALUES (N'ff7ecdbb-2ac8-4fe4-95fb-897dd5fbe27c', N'Info', N'LabDEProvider', N'The LabDEProvider table will have 0 rows updated.', '20201223 12:15:22.517', NULL, NULL, N'Update')
ALTER TABLE [dbo].[WorkflowChange]
    ADD CONSTRAINT [FK_WorkflowChange_WorkflowDest] FOREIGN KEY ([DestWorkflowID]) REFERENCES [dbo].[Workflow] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ActionStatisticsDelta]
    ADD CONSTRAINT [FK_ActionStatisticsDelta_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkflowChangeFile]
    ADD CONSTRAINT [FK_WorkflowChangeFile_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkflowChangeFile]
    ADD CONSTRAINT [FK_WorkflowChangeFile_WorkflowChange] FOREIGN KEY ([WorkflowChangeID]) REFERENCES [dbo].[WorkflowChange] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupReport]
    ADD CONSTRAINT [FK_GroupReport_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ActionStatistics]
    ADD CONSTRAINT [FK_Statistics_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[LoginGroupMembership]
    ADD CONSTRAINT [FK_LoginGroupMembership_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[LoginGroupMembership]
    ADD CONSTRAINT [FK_LoginGroupMembership_LOGIN_ID] FOREIGN KEY ([LoginID]) REFERENCES [dbo].[Login] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileHandler]
    ADD CONSTRAINT [FK_FileHandler_Workflow] FOREIGN KEY ([WorkflowName]) REFERENCES [dbo].[Workflow] ([Name]) ON DELETE SET NULL ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkItemGroup]
    ADD CONSTRAINT [FK_WorkItemGroup_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkItemGroup]
    ADD CONSTRAINT [FK_WorkItemGroup_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkItemGroup]
    ADD CONSTRAINT [FK_WorkItemGroup_FAMSession] FOREIGN KEY ([FAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [dbo].[DBInfoChangeHistory]
    ADD CONSTRAINT [FK_DBInfoChangeHistory_User] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DBInfoChangeHistory]
    ADD CONSTRAINT [FK_DBInfoChangeHistory_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DBInfoChangeHistory]
    ADD CONSTRAINT [FK_DBInfoChageHistory_DBInfo] FOREIGN KEY ([DBInfoID]) REFERENCES [dbo].[DBInfo] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkItem]
    ADD CONSTRAINT [FK_WorkItem_WorkItemGroup] FOREIGN KEY ([WorkItemGroupID]) REFERENCES [dbo].[WorkItemGroup] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkItem]
    ADD CONSTRAINT [FK_WorkItem_FAMSession] FOREIGN KEY ([FAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [Security].[GroupAction]
    ADD CONSTRAINT [FK_GroupAction_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupAction]
    ADD CONSTRAINT [FK_GroupAction_Action_ID] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FTPEventHistory]
    ADD CONSTRAINT [FK_FTPEventHistory_FTPAccount] FOREIGN KEY ([FTPAccountID]) REFERENCES [dbo].[FTPAccount] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FTPEventHistory]
    ADD CONSTRAINT [FK_FTPEventHistory_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FTPEventHistory]
    ADD CONSTRAINT [FK_FTPEventHistory_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FTPEventHistory]
    ADD CONSTRAINT [FK_FTPEventHistory_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FTPEventHistory]
    ADD CONSTRAINT [FK_FTPEventHistory_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[IDShieldData]
    ADD CONSTRAINT [FK_IDShieldData_FileTaskSession] FOREIGN KEY ([FileTaskSessionID]) REFERENCES [dbo].[FileTaskSession] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileMetadataFieldValue]
    ADD CONSTRAINT [FK_FileMetadataFieldValue_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileMetadataFieldValue]
    ADD CONSTRAINT [FK_FileMetadataFieldValue_MetadataField] FOREIGN KEY ([MetadataFieldID]) REFERENCES [dbo].[MetadataField] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileTag]
    ADD CONSTRAINT [FK_FileTag_FamFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileTag]
    ADD CONSTRAINT [FK_FileTag_Tag] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tag] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    ADD CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Expected] FOREIGN KEY ([ExpectedAttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID])
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    ADD CONSTRAINT [FK_ReportingRedactionAccuracy_AttributeSetForFile_Found] FOREIGN KEY ([FoundAttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID])
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    ADD CONSTRAINT [FK_ReportingRedactionAccuracy_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    ADD CONSTRAINT [FK_ReportingRedactionAccuracy_DatabaseService] FOREIGN KEY ([DatabaseServiceID]) REFERENCES [dbo].[DatabaseService] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingRedactionAccuracy_FAMUser_Found] FOREIGN KEY ([FoundFAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingRedactionAccuracy_FAMUser_Expected] FOREIGN KEY ([ExpectedFAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingRedactionAccuracy_Action_Found] FOREIGN KEY ([FoundActionID]) REFERENCES [dbo].[Action] ([ID])
ALTER TABLE [dbo].[ReportingRedactionAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingRedactionAccuracy_Action_Expected] FOREIGN KEY ([ExpectedActionID]) REFERENCES [dbo].[Action] ([ID])
ALTER TABLE [dbo].[LabDEEncounterFile]
    ADD CONSTRAINT [FK_EncounterFile_EncounterID] FOREIGN KEY ([EncounterID]) REFERENCES [dbo].[LabDEEncounter] ([CSN]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEEncounterFile]
    ADD CONSTRAINT [FK_EncounterFile_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileTaskSessionCache]
    ADD CONSTRAINT [FK_FileTaskSessionCache_ActiveFAM] FOREIGN KEY ([AutoDeleteWithActiveFAMID]) REFERENCES [dbo].[ActiveFAM] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ActiveFAM]
    ADD CONSTRAINT [FK_ActiveFAM_FAMSession] FOREIGN KEY ([FAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [Security].[GroupRole]
    ADD CONSTRAINT [FK_GroupRole_Role_ID] FOREIGN KEY ([RoleGUID]) REFERENCES [Security].[Role] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupRole]
    ADD CONSTRAINT [FK_GroupRole_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEOrderFile]
    ADD CONSTRAINT [FK_OrderFile_Order] FOREIGN KEY ([OrderNumber]) REFERENCES [dbo].[LabDEOrder] ([OrderNumber]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEOrderFile]
    ADD CONSTRAINT [FK_OrderFile_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FAMSession]
    ADD CONSTRAINT [FK_FAMSession_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE SET NULL ON UPDATE CASCADE
ALTER TABLE [dbo].[FAMSession]
    ADD CONSTRAINT [FK_FAMSession_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FAMSession]
    ADD CONSTRAINT [FK_FAMSession_User] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FAMSession]
    ADD CONSTRAINT [FK_FAMSession_FPSFile] FOREIGN KEY ([FPSFileID]) REFERENCES [dbo].[FPSFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[SecureCounterValueChange]
    WITH NOCHECK ADD CONSTRAINT [FK_SecureCounterValueChange_FAMSession] FOREIGN KEY ([LastUpdatedByFAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [dbo].[SecureCounterValueChange]
    ADD CONSTRAINT [FK_SecureCounterValueChange_SecureCounter] FOREIGN KEY ([CounterID]) REFERENCES [dbo].[SecureCounter] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueueEvent]
    ADD CONSTRAINT [FK_QueueEvent_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[QueueEvent]
    ADD CONSTRAINT [FK_QueueEvent_QueueEventCode] FOREIGN KEY ([QueueEventCode]) REFERENCES [dbo].[QueueEventCode] ([Code])
ALTER TABLE [dbo].[QueueEvent]
    ADD CONSTRAINT [FK_QueueEvent_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID])
ALTER TABLE [dbo].[QueueEvent]
    ADD CONSTRAINT [FK_QueueEvent_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[QueueEvent]
    ADD CONSTRAINT [FK_QueueEvent_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[MLData]
    ADD CONSTRAINT [FK_MLData_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[MLData]
    ADD CONSTRAINT [FK_MLData_MLModel] FOREIGN KEY ([MLModelID]) REFERENCES [dbo].[MLModel] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[InputEvent]
    ADD CONSTRAINT [FK_InputEvent_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[InputEvent]
    ADD CONSTRAINT [FK_InputEvent_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[InputEvent]
    ADD CONSTRAINT [FK_InputEvent_User] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[AttributeInstanceType]
    ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeID] FOREIGN KEY ([AttributeID]) REFERENCES [dbo].[Attribute] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[AttributeInstanceType]
    ADD CONSTRAINT [FK_Attribute_Instance_Type_AttributeTypeID] FOREIGN KEY ([AttributeTypeID]) REFERENCES [dbo].[AttributeType] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Pagination]
    ADD CONSTRAINT [FK_Pagination_SourceFile_FAMFile] FOREIGN KEY ([SourceFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[Pagination]
    ADD CONSTRAINT [FK_Pagination_DestFile_FAMFile] FOREIGN KEY ([DestFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[Pagination]
    ADD CONSTRAINT [FK_Pagination_OriginalFile_FAMFile] FOREIGN KEY ([OriginalFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[Pagination]
    ADD CONSTRAINT [FK_Pagination_FileTaskSession] FOREIGN KEY ([FileTaskSessionID]) REFERENCES [dbo].[FileTaskSession] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueuedActionStatusChange]
    ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueuedActionStatusChange]
    ADD CONSTRAINT [FK_QueuedActionStatusChange_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueuedActionStatusChange]
    ADD CONSTRAINT [FK_QueuedActionStatusChange_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueuedActionStatusChange]
    ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[QueuedActionStatusChange]
    ADD CONSTRAINT [FK_QueuedActionStatusChange_FAMSession] FOREIGN KEY ([FAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [dbo].[LockedFile]
    ADD CONSTRAINT [FK_LockedFile_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LockedFile]
    ADD CONSTRAINT [FK_LockedFile_ActionState] FOREIGN KEY ([StatusBeforeLock]) REFERENCES [dbo].[ActionState] ([Code]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LockedFile]
    ADD CONSTRAINT [FK_LockedFile_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LockedFile]
    ADD CONSTRAINT [FK_LockedFile_ActiveFAM] FOREIGN KEY ([ActiveFAMID]) REFERENCES [dbo].[ActiveFAM] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_To] FOREIGN KEY ([ASC_To]) REFERENCES [dbo].[ActionState] ([Code])
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_ActionState_From] FOREIGN KEY ([ASC_From]) REFERENCES [dbo].[ActionState] ([Code])
ALTER TABLE [dbo].[FileActionStateTransition]
    ADD CONSTRAINT [FK_FileActionStateTransition_Queue] FOREIGN KEY ([QueueID]) REFERENCES [dbo].[QueuedActionStatusChange] ([ID])
ALTER TABLE [Security].[GroupDashboard]
    ADD CONSTRAINT [FK_GroupDashboard_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupDashboard]
    ADD CONSTRAINT [FK_GroupDashboard_Dashboard_Guid] FOREIGN KEY ([DashboardGUID]) REFERENCES [dbo].[Dashboard] ([Guid]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Dashboard]
    ADD CONSTRAINT [FK_Dashboard_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FAMUser]
    ADD CONSTRAINT [FK_FAMUSER_LOGIN_ID] FOREIGN KEY ([LoginID]) REFERENCES [dbo].[Login] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEEncounter]
    ADD CONSTRAINT [FK_Encounter_Patient] FOREIGN KEY ([PatientMRN]) REFERENCES [dbo].[LabDEPatient] ([MRN]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Expected] FOREIGN KEY ([ExpectedAttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID])
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_AttributeSetForFile_Found] FOREIGN KEY ([FoundAttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID])
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_DatabaseService] FOREIGN KEY ([DatabaseServiceID]) REFERENCES [dbo].[DatabaseService] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMUser_Found] FOREIGN KEY ([FoundFAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_FAMUser_Expected] FOREIGN KEY ([ExpectedFAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_Action_Found] FOREIGN KEY ([FoundActionID]) REFERENCES [dbo].[Action] ([ID])
ALTER TABLE [dbo].[ReportingDataCaptureAccuracy]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingDataCaptureAccuracy_Action_Expected] FOREIGN KEY ([ExpectedActionID]) REFERENCES [dbo].[Action] ([ID])
ALTER TABLE [dbo].[LabDEOrder]
    ADD CONSTRAINT [FK_Order_Patient] FOREIGN KEY ([PatientMRN]) REFERENCES [dbo].[LabDEPatient] ([MRN]) ON DELETE SET NULL ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEOrder]
    ADD CONSTRAINT [FK_Order_OrderStatus] FOREIGN KEY ([OrderStatus]) REFERENCES [dbo].[LabDEOrderStatus] ([Code]) ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEOrder]
    ADD CONSTRAINT [FK_Order_Encounter] FOREIGN KEY ([EncounterID]) REFERENCES [dbo].[LabDEEncounter] ([CSN])
ALTER TABLE [dbo].[DocTagHistory]
    ADD CONSTRAINT [FK_DocTagHistory_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DocTagHistory]
    ADD CONSTRAINT [FK_DocTagHistory_Tag] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tag] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DocTagHistory]
    ADD CONSTRAINT [FK_DocTagHistory_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DocTagHistory]
    ADD CONSTRAINT [FK_DocTagHistory_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionComment]
    ADD CONSTRAINT [FK_FileActionComment_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionComment]
    ADD CONSTRAINT [FK_FileActionComment_FAMFILE] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Action]
    ADD CONSTRAINT [FK_Action_Workflow] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflow] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileTaskSession]
    ADD CONSTRAINT [FK_FileTaskSession_FAMSession] FOREIGN KEY ([FAMSessionID]) REFERENCES [dbo].[FAMSession] ([ID])
ALTER TABLE [dbo].[FileTaskSession]
    ADD CONSTRAINT [FK_FileTaskSession_TaskClass] FOREIGN KEY ([TaskClassID]) REFERENCES [dbo].[TaskClass] ([ID])
ALTER TABLE [dbo].[FileTaskSession]
    ADD CONSTRAINT [FK_FileTaskSession_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[RasterZone]
    ADD CONSTRAINT [FK_RasterZone_AttributeID] FOREIGN KEY ([AttributeID]) REFERENCES [dbo].[Attribute] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Attribute]
    ADD CONSTRAINT [FK_Attribute_AttributeSetForFileID] FOREIGN KEY ([AttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Attribute]
    ADD CONSTRAINT [FK_Attribute_AttributeNameID] FOREIGN KEY ([AttributeNameID]) REFERENCES [dbo].[AttributeName] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[Attribute]
    ADD CONSTRAINT [FK_Attribute_ParentAttributeID] FOREIGN KEY ([ParentAttributeID]) REFERENCES [dbo].[Attribute] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_Pagination] FOREIGN KEY ([PaginationID]) REFERENCES [dbo].[Pagination] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_FAMUser] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_FAMFile_SourceFile] FOREIGN KEY ([SourceFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_FAMFile_DestFile] FOREIGN KEY ([DestFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_FAMFile_OriginalFile] FOREIGN KEY ([OriginalFileID]) REFERENCES [dbo].[FAMFile] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_FileTaskSession] FOREIGN KEY ([FileTaskSessionID]) REFERENCES [dbo].[FileTaskSession] ([ID])
ALTER TABLE [dbo].[ReportingHIMStats]
    WITH NOCHECK ADD CONSTRAINT [FK_ReportingHIMStats_ActionID] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID])
ALTER TABLE [dbo].[FileActionStatus]
    ADD CONSTRAINT [FK_FileActionStatus_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStatus]
    ADD CONSTRAINT [FK_FileActionStatus_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[FileActionStatus]
    ADD CONSTRAINT [FK_FileActionStatus_ActionStatus] FOREIGN KEY ([ActionStatus]) REFERENCES [dbo].[ActionState] ([Code]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DashboardAttributeFields]
    ADD CONSTRAINT [FK_DashboardAttributeFields_AttributeSetForFileID] FOREIGN KEY ([AttributeSetForFileID]) REFERENCES [dbo].[AttributeSetForFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[SourceDocChangeHistory]
    ADD CONSTRAINT [FK_SourceDocChangeHistory_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[SourceDocChangeHistory]
    ADD CONSTRAINT [FK_SourceDocChangeHistory_User] FOREIGN KEY ([FAMUserID]) REFERENCES [dbo].[FAMUser] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[SourceDocChangeHistory]
    ADD CONSTRAINT [FK_SourceDocChangeHistory_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkflowFile]
    ADD CONSTRAINT [FK_WorkflowFile_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[WorkflowFile]
    ADD CONSTRAINT [FK_WorkflowFile_Workflow] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflow] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[DatabaseService]
    ADD CONSTRAINT [FK_DatabaseService_Machine] FOREIGN KEY ([MachineID]) REFERENCES [dbo].[Machine] ([ID]) ON DELETE SET NULL ON UPDATE CASCADE
ALTER TABLE [dbo].[DatabaseService]
    ADD CONSTRAINT [FK_DatabaseService_ActiveFAM] FOREIGN KEY ([ActiveFAMID]) REFERENCES [dbo].[ActiveFAM] ([ID]) ON DELETE SET NULL ON UPDATE CASCADE
ALTER TABLE [dbo].[DatabaseService]
    ADD CONSTRAINT [FK_DatabaseService_Active_Machine] FOREIGN KEY ([ActiveServiceMachineID]) REFERENCES [dbo].[Machine] ([ID])
ALTER TABLE [dbo].[LabDEPatientFile]
    ADD CONSTRAINT [FK_PatientFile_PatientMRN] FOREIGN KEY ([PatientMRN]) REFERENCES [dbo].[LabDEPatient] ([MRN]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEPatientFile]
    ADD CONSTRAINT [FK_PatientFile_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[LabDEPatient]
    ADD CONSTRAINT [FK_PatientMergedInto] FOREIGN KEY ([MergedInto]) REFERENCES [dbo].[LabDEPatient] ([MRN])
ALTER TABLE [dbo].[LabDEPatient]
    ADD CONSTRAINT [FK_PatientCurrentMRN] FOREIGN KEY ([CurrentMRN]) REFERENCES [dbo].[LabDEPatient] ([MRN])
ALTER TABLE [dbo].[ReportingVerificationRates]
    ADD CONSTRAINT [FK_ReportingVerificationRates_FAMFile] FOREIGN KEY ([FileID]) REFERENCES [dbo].[FAMFile] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingVerificationRates]
    ADD CONSTRAINT [FK_ReportingVerificationRates_DatabaseService] FOREIGN KEY ([DatabaseServiceID]) REFERENCES [dbo].[DatabaseService] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingVerificationRates]
    ADD CONSTRAINT [FK_ReportingVerificationRates_Action] FOREIGN KEY ([ActionID]) REFERENCES [dbo].[Action] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingVerificationRates]
    ADD CONSTRAINT [FK_ReportingVerificationRates_TaskClass] FOREIGN KEY ([TaskClassID]) REFERENCES [dbo].[TaskClass] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[ReportingVerificationRates]
    ADD CONSTRAINT [FK_ReportingVerificationRates_FileTaskSession] FOREIGN KEY ([LastFileTaskSessionID]) REFERENCES [dbo].[FileTaskSession] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupWorkflow]
    ADD CONSTRAINT [FK_GroupWorkflow_Group_ID] FOREIGN KEY ([GroupGUID]) REFERENCES [Security].[Group] ([GUID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [Security].[GroupWorkflow]
    ADD CONSTRAINT [FK_GroupWorkflow_WORKFLOW_ID] FOREIGN KEY ([WorkflowID]) REFERENCES [dbo].[Workflow] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[AttributeSetForFile]
    ADD CONSTRAINT [FK_AttributeSetForFile_FileTaskSessionID] FOREIGN KEY ([FileTaskSessionID]) REFERENCES [dbo].[FileTaskSession] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE [dbo].[AttributeSetForFile]
    ADD CONSTRAINT [FK_AttributeSetForFile_AttributeSetNameID] FOREIGN KEY ([AttributeSetNameID]) REFERENCES [dbo].[AttributeSetName] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE
COMMIT TRANSACTION
