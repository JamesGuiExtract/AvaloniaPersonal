﻿using DatabaseMigrationWizard.Database.Input.DataTransformObject;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DatabaseMigrationWizard.Test
{
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotDeclareVisibleInstanceFields", Justification = "This is a test class.", Scope = "namespaceanddescendants")]
    public class DatabaseMigrationWizardTestHelper
    {
        public Collection<Database.Input.DataTransformObject.Action> Actions { get; } = new Collection<Database.Input.DataTransformObject.Action>();

        public Collection<AttributeName> AttributeNames { get; } = new Collection<AttributeName>();
               
        public Collection<AttributeSetName> AttributeSetNames { get; } = new Collection<AttributeSetName>();
               
        public Collection<Dashboard> Dashboards { get; } = new Collection<Dashboard>();
               
        public Collection<DatabaseService> DatabaseServices { get; } = new Collection<DatabaseService>();
               
        public Collection<DataEntryCounterDefinition> DataEntryCounterDefinitions { get; } = new Collection<DataEntryCounterDefinition>();
               
        public Collection<DBInfo> DBInfos { get; } = new Collection<DBInfo>();
               
        public Collection<FAMUser> FAMUsers { get; } = new Collection<FAMUser>();
               
        public Collection<FieldSearch> FieldSearches { get; } = new Collection<FieldSearch>();
               
        public Collection<FileHandler> FileHandlers { get; } = new Collection<FileHandler>();
               
        public Collection<LabDEEncounter> LabDEEncounters { get; } = new Collection<LabDEEncounter>();
               
        public Collection<LabDEOrder> LabDEOrders { get; } = new Collection<LabDEOrder>();
               
        public Collection<LabDEPatient> LabDEPatients { get; } = new Collection<LabDEPatient>();
               
        public Collection<LabDEProvider> LabDEProviders { get; } = new Collection<LabDEProvider>();
               
        public Collection<Login> Logins { get; } = new Collection<Login>();
               
        public Collection<MetadataField> MetadataFields { get; } = new Collection<MetadataField>();
               
        public Collection<MLModel> MLModels { get; } = new Collection<MLModel>();
               
        public Collection<Tag> Tags { get; } = new Collection<Tag>();
               
        public Collection<UserCreatedCounter> UserCreatedCounters { get; } = new Collection<UserCreatedCounter>();
               
        public Collection<WebAppConfig> WebAppConfigurations { get; } = new Collection<WebAppConfig>();
               
        public Collection<Workflow> Workflows { get; } = new Collection<Workflow>();

        public void WriteEverythingToDirectory(string directory)
        {
            WriteTablesHelper<Database.Input.DataTransformObject.Action>(directory + "\\Action.json", this.Actions);
            WriteTablesHelper<AttributeName>(directory + "\\AttributeName.json", this.AttributeNames);
            WriteTablesHelper<AttributeSetName>(directory + "\\AttributeSetName.json", this.AttributeSetNames);
            WriteTablesHelper<Dashboard>(directory + "\\Dashboard.json", this.Dashboards);
            WriteTablesHelper<DatabaseService>(directory + "\\DatabaseService.json", this.DatabaseServices);
            WriteTablesHelper<DataEntryCounterDefinition>(directory + "\\DataEntryCounterDefinition.json", this.DataEntryCounterDefinitions);
            WriteTablesHelper<DBInfo>(directory + "\\DBInfo.json", this.DBInfos);
            WriteTablesHelper<FAMUser>(directory + "\\FAMUser.json", this.FAMUsers);
            WriteTablesHelper<FieldSearch>(directory + "\\FieldSearch.json", this.FieldSearches);
            WriteTablesHelper<FileHandler>(directory + "\\FileHandler.json", this.FileHandlers);
            WriteTablesHelper<LabDEEncounter>(directory + "\\LabDEEncounter.json", this.LabDEEncounters);
            WriteTablesHelper<LabDEOrder>(directory + "\\LabDEOrder.json", this.LabDEOrders);
            WriteTablesHelper<LabDEPatient>(directory + "\\LabDEPatient.json", this.LabDEPatients);
            WriteTablesHelper<LabDEProvider>(directory + "\\LabDEProvider.json", this.LabDEProviders);
            WriteTablesHelper<Login>(directory + "\\Login.json", this.Logins);
            WriteTablesHelper<MetadataField>(directory + "\\MetadataField.json", this.MetadataFields);
            WriteTablesHelper<MLModel>(directory + "\\MLModel.json", this.MLModels);
            WriteTablesHelper<Tag>(directory + "\\Tag.json", this.Tags);
            WriteTablesHelper<UserCreatedCounter>(directory + "\\UserCreatedCounter.json", this.UserCreatedCounters);
            WriteTablesHelper<WebAppConfig>(directory + "\\WebAppConfig.json", this.WebAppConfigurations);
            WriteTablesHelper<Workflow>(directory + "\\Workflow.json", this.Workflows);
        }

        private static void WriteTablesHelper<T>(string directory, Collection<T> items)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            using (StreamWriter sw = new StreamWriter(directory))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                foreach (var item in items)
                {
                    serializer.Serialize(writer, item);
                }
            }
        }

        public void LoadInitialValues()
        {
            Actions.Add(new Database.Input.DataTransformObject.Action() { ASCName = "TestAction", Description = "This is a test description", ActionGuid = Guid.Parse("36f534f9-0589-431a-845b-639b0720b4ad") });
            Actions.Add(new Database.Input.DataTransformObject.Action() { ASCName = "TestAction", MainSequence = true, ActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"), WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520") });
            Actions.Add(new Database.Input.DataTransformObject.Action() { ASCName = "TestAction2", ActionGuid = Guid.Parse("ade5baae-0dae-4452-9679-0da6c9c4bf80") });
            Actions.Add(new Database.Input.DataTransformObject.Action() { ASCName = "TestAction2", MainSequence = false, ActionGuid = Guid.Parse("cd27650d-dfe4-44c3-9fc8-96ea99f7a4e2"), WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520") });

            AttributeNames.Add(new AttributeName() { Name = "DocumentType", Guid = Guid.Parse("1bdf026e-1a8a-4e61-92e2-60f13f06c984") });
            AttributeNames.Add(new AttributeName() { Name = "PatientInfo", Guid = Guid.Parse("8d0731a3-482e-4a8e-8df4-8a99789400fa") });

            AttributeSetNames.Add(new AttributeSetName() { Description = "TestAttributeSet", Guid = Guid.Parse("7c081610-2f63-4f0c-9a3b-d018176bd5ea") });
            AttributeSetNames.Add(new AttributeSetName() { Description = "Reassign", Guid = Guid.Parse("8dbc6db1-cd76-4329-80f9-74afbc02dd15") });

            Dashboards.Add(new Dashboard()
            {
                DashboardName = "DashboardTest",
                Definition = "<Dashboard CurrencyCulture=\"en-US\"><Title Text=\"Dashboard\" /><DataSources><SqlDataSource Name=\"SQL Data Source 1\" ComponentName=\"dashboardSqlDataSource1\" DataProcessingMode=\"Client\"><Connection Name=\"localhost_Essentia_Customer_Builds_Connection\" ProviderKey=\"MSSqlServer\"><Parameters><Parameter Name=\"server\" Value=\"zeus\" /><Parameter Name=\"database\" Value=\"Essentia_Customer_Builds\" /><Parameter Name=\"useIntegratedSecurity\" Value=\"True\" /><Parameter Name=\"read only\" Value=\"1\" /><Parameter Name=\"generateConnectionHelper\" Value=\"false\" /><Parameter Name=\"userid\" Value=\"\" /><Parameter Name=\"password\" Value=\"\" /></Parameters></Connection><Query Type=\"CustomSqlQuery\" Name=\"Query\"><Parameter Name=\"ReportingPeroid\" Type=\"DevExpress.DataAccess.Expression\">(System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)(GetDate(Iif(?ReportingPeroid = '2 Weeks', AddDays(Today(), -14), ?ReportingPeroid = '1 Month', AddMonths(Today(), -1), ?ReportingPeroid = '2 Months', AddMonths(Today(), -3), ?ReportingPeroid = '6 Months', AddMonths(Today(), -6), ?ReportingPeroid = '1 Year', AddYears(Today(), -1), ?ReportingPeroid = 'Since Go Live (9/25/2018)', GetDate('9/25/2018 12:00AM'), AddDays(Today(), -14)))\r\n)</Parameter><Sql>--DECLARE @REPORTINGPEROID DATE;\r\n--SET @REPORTINGPEROID = '1/20/2016';\r\n\r\nSELECT\r\n\tCTE.FullUserName\r\n\t, COUNT( DISTINCT OriginalFileID) AS InputDocumentCount\r\n\t, COUNT( DISTINCT CTE.DestFileID) AS OutputDocumentCount\r\n\t, CTE.InputDate\r\n\t, MAX(CurrentlyActive) AS CurrentlyActive\r\n\t, SUM( TotalMinutes) AS ActiveMinutes\r\n\t, CTE.Name\r\n\t, SUM( Pages) AS TotalPages\r\n\t, SUM(Correct) AS CorrectSum\r\n\t, SUM(Expected) AS ExpectedSum\r\n\t, SUM(Expected) - SUM(Correct) AS IncorrectPlusMissed\r\nFROM\r\n\t( -- This is necessary for getting the total minutes and pages. These values should only be applied once per doc, hence the rownum\r\n\tSELECT DISTINCT\r\n\t\tdbo.FAMUser.FullUserName\r\n\t\t, ReportingHIMStats.OriginalFileID\r\n\t\t, ReportingHIMStats.DestFileID\r\n\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t, vUsersWithActive.CurrentlyActive\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY TotalMinutes ORDER BY TotalMinutes ) = 1 THEN TotalMinutes\r\n\t\t\tELSE 0\r\n\t\t  END AS TotalMinutes\r\n\t\t, Workflow.Name\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY dbo.FAMFile.Pages, dbo.FAMFile.ID ORDER BY dbo.FAMFile.Pages desc ) = 1 THEN dbo.FAMFile.Pages\r\n\t\t\tELSE 0\r\n\t\t  END AS Pages\r\n\r\n\tFROM\r\n\t\tdbo.FAMUser\r\n\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.FAMFile\r\n\t\t\t\t\tON dbo.FAMFile.ID = ReportingHIMStats.OriginalFileID\r\n\r\n\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\tINNER JOIN dbo.vUsersWithActive \r\n\t\t\t\t\t\tON vUsersWithActive.FAMUserID = vFAMUserInputEventsTime.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\tWHERE\r\n\t\tFAMUser.FullUserName IS NOT NULL\r\n\t) AS CTE\r\n\t\tLEFT OUTER JOIN ( --This subquery is necessary for reporing on the ReportingDataCaptureAccuracy. They require a different grouping than pages/active hours\r\n\t\t\t\t\t\t\tSELECT DISTINCT\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Correct) AS Correct\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Expected) AS Expected\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\tdbo.FAMUser\r\n\t\t\t\t\t\t\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\t\t\t\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\t\t\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\t\t\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\t\t\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\t\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\t\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\t\t\t\t\t\t\t\t\t\tLEFT OUTER JOIN dbo.ReportingDataCaptureAccuracy\r\n\t\t\t\t\t\t\t\t\t\t\tON ReportingDataCaptureAccuracy.FileID = ReportingHIMStats.DestFileID\r\n\r\n\t\t\t\t\t\t\tWHERE\r\n\t\t\t\t\t\t\t\tFAMUser.FullUserName IS NOT NULL\r\n\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\t\t\t\t\t\t\t) AS CTE2\r\n\t\t\t\t\t\t\t\tON CTE2.DestFileID = CTE.DestFileID\r\n\t\t\t\t\t\t\t\tAND CTE2.FullUserName = CTE.FullUserName\r\n\t\t\t\t\t\t\t\tAND CTE.InputDate = CTE2.InputDate\r\n\t\t\t\t\t\t\t\tAND CTE.Name = CTE2.Name\r\nGROUP BY\r\n\tCTE.FullUserName\r\n\t, CTE.InputDate\r\n\t, CTE.Name\r\n</Sql></Query><ResultSchema><DataSet Name=\"SQL Data Source 1\"><View Name=\"Query\"><Field Name=\"FullUserName\" Type=\"String\" /><Field Name=\"InputDocumentCount\" Type=\"Int32\" /><Field Name=\"OutputDocumentCount\" Type=\"Int32\" /><Field Name=\"InputDate\" Type=\"DateTime\" /><Field Name=\"CurrentlyActive\" Type=\"Int32\" /><Field Name=\"ActiveMinutes\" Type=\"Double\" /><Field Name=\"Name\" Type=\"String\" /><Field Name=\"TotalPages\" Type=\"Int32\" /><Field Name=\"CorrectSum\" Type=\"Int64\" /><Field Name=\"ExpectedSum\" Type=\"Int64\" /><Field Name=\"IncorrectPlusMissed\" Type=\"Int64\" /></View></DataSet></ResultSchema><ConnectionOptions CloseConnection=\"true\" DbCommandTimeout=\"0\" /><CalculatedFields><CalculatedField Name=\"Pages/Hour\" Expression=\"Sum([TotalPages]) / (SUM([ActiveMinutes]) / 60)\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"ActiveHours\" Expression=\"SUM([ActiveMinutes]) / 60\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"Documents/Hour\" Expression=\"SUM([OutputDocumentCount]) / [ActiveHours]\" DataType=\"Auto\" DataMember=\"Query\" /></CalculatedFields></SqlDataSource></DataSources><Parameters><Parameter Name=\"ReportingPeroid\" Value=\"2 Weeks\"><StaticListLookUpSettings><Values><Value>2 Weeks</Value><Value>1 Month</Value><Value>2 Months</Value><Value>6 Months</Value><Value>1 Year</Value><Value>Since Go Live (9/25/2018)</Value></Values></StaticListLookUpSettings></Parameter></Parameters><Items><Card ComponentName=\"cardDashboardItem1\" Name=\"Cards 1\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"InputDocumentCount\" Name=\"Input Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem2\" Name=\"Cards 2\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem1\" Name=\"User Productivity\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Measure DataMember=\"CurrentlyActive\" Name=\"Active\" DefaultId=\"DataItem0\" /><Dimension DataMember=\"FullUserName\" DefaultId=\"DataItem1\" /><Measure DataMember=\"InputDocumentCount\" Name=\"Input Docs\" DefaultId=\"DataItem2\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Docs\" DefaultId=\"DataItem3\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ActiveMinutes\" Name=\"Active Minutes\" DefaultId=\"DataItem4\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem5\" /><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem6\" /><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem7\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ExpectedSum\" Name=\"Indexed Fields\" DefaultId=\"DataItem8\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"CorrectSum\" Name=\"Auto-Populated\" DefaultId=\"DataItem9\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"IncorrectPlusMissed\" DefaultId=\"DataItem10\" /></DataItems><FormatRules><GridItemFormatRule Name=\"FormatRule 1\" DataItem=\"DataItem0\"><FormatConditionRangeSet ValueType=\"Number\"><RangeSet><Ranges><RangeInfo><Value Type=\"System.Decimal\" Value=\"0\" /><IconSettings IconType=\"ShapeRedCircle\" /></RangeInfo><RangeInfo><Value Type=\"System.Int32\" Value=\"1\" /><IconSettings IconType=\"ShapeGreenCircle\" /></RangeInfo></Ranges></RangeSet></FormatConditionRangeSet></GridItemFormatRule></FormatRules><GridColumns><GridMeasureColumn Weight=\"29.785894206549106\"><Measure DefaultId=\"DataItem0\" /></GridMeasureColumn><GridDimensionColumn Weight=\"90.050377833753117\"><Dimension DefaultId=\"DataItem1\" /></GridDimensionColumn><GridMeasureColumn Weight=\"83.816120906800975\"><Measure DefaultId=\"DataItem2\" /></GridMeasureColumn><GridMeasureColumn Weight=\"63.72795969773297\"><Measure DefaultId=\"DataItem3\" /></GridMeasureColumn><GridMeasureColumn Weight=\"78.967254408060413\"><Measure DefaultId=\"DataItem7\" /></GridMeasureColumn><GridMeasureColumn Weight=\"70.654911838790909\"><Measure DefaultId=\"DataItem4\" /></GridMeasureColumn><GridMeasureColumn Weight=\"95.591939546599463\"><Measure DefaultId=\"DataItem8\" /></GridMeasureColumn><GridMeasureColumn Weight=\"86.586901763224148\"><Measure DefaultId=\"DataItem9\" /></GridMeasureColumn><GridMeasureColumn Weight=\"60.9571788413098\"><Measure DefaultId=\"DataItem10\" /></GridMeasureColumn><GridMeasureColumn Weight=\"89.357682619647321\"><Measure DefaultId=\"DataItem6\" /></GridMeasureColumn><GridMeasureColumn Weight=\"75.503778337531443\"><Measure DefaultId=\"DataItem5\" /></GridMeasureColumn></GridColumns><GridOptions EnableBandedRows=\"true\" ColumnWidthMode=\"Manual\" /></Grid><RangeFilter ComponentName=\"rangeFilterDashboardItem1\" Name=\"Filter by Date\" ShowCaption=\"true\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Dimension DataMember=\"InputDate\" DateTimeGroupInterval=\"DayMonthYear\" DefaultId=\"DataItem0\" /><Measure DataMember=\"TotalPages\" DefaultId=\"DataItem1\" /></DataItems><Argument DefaultId=\"DataItem0\" /><Series><Simple SeriesType=\"Line\"><Value DefaultId=\"DataItem1\" /></Simple></Series></RangeFilter><Card ComponentName=\"cardDashboardItem3\" Name=\"Cards 3\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem4\" Name=\"Cards 4\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"ActiveHours\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem5\" Name=\"Cards 5\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem6\" Name=\"Cards 6\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem2\" Name=\"By Queue\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Dimension DataMember=\"Name\" DefaultId=\"DataItem0\" /><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem1\" /></DataItems><GridColumns><GridDimensionColumn><Dimension DefaultId=\"DataItem0\" /></GridDimensionColumn><GridMeasureColumn><Measure DefaultId=\"DataItem1\" /></GridMeasureColumn></GridColumns><GridOptions /></Grid></Items><LayoutTree><LayoutGroup Orientation=\"Vertical\" Weight=\"100\"><LayoutGroup Weight=\"16.867469879518072\"><LayoutItem DashboardItem=\"cardDashboardItem1\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem2\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem5\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"19.277108433734941\"><LayoutItem DashboardItem=\"cardDashboardItem4\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem6\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem3\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"29.036144578313252\"><LayoutItem DashboardItem=\"gridDashboardItem2\" Weight=\"19.761273209549071\" /><LayoutItem DashboardItem=\"gridDashboardItem1\" Weight=\"80.238726790450926\" /></LayoutGroup><LayoutItem DashboardItem=\"rangeFilterDashboardItem1\" Weight=\"34.819277108433738\" /></LayoutGroup></LayoutTree></Dashboard>",
                LastImportedDate = "2020-03-13T12:46:02.203",
                UseExtractedData = false,
                ExtractedDataDefinition = null,
                DashboardGuid = Guid.Parse("e9ecb43e-2ff4-42a3-82c0-6cfc4e09e100"),
                UserName = "Trever_Gannon",
                FullUserName = "DataMaster"
            });
            Dashboards.Add(new Dashboard()
            {
                DashboardName = "DashboardTestLamo",
                Definition = "<Dashboard CurrencyCulture=\"en-US\"><Title Text=\"Dashboard\" /><DataSources><SqlDataSource Name=\"SQL Data Source 1\" ComponentName=\"dashboardSqlDataSource1\" DataProcessingMode=\"Client\"><Connection Name=\"localhost_Essentia_Customer_Builds_Connection\" ProviderKey=\"MSSqlServer\"><Parameters><Parameter Name=\"server\" Value=\"zeus\" /><Parameter Name=\"database\" Value=\"Essentia_Customer_Builds\" /><Parameter Name=\"useIntegratedSecurity\" Value=\"True\" /><Parameter Name=\"read only\" Value=\"1\" /><Parameter Name=\"generateConnectionHelper\" Value=\"false\" /><Parameter Name=\"userid\" Value=\"\" /><Parameter Name=\"password\" Value=\"\" /></Parameters></Connection><Query Type=\"CustomSqlQuery\" Name=\"Query\"><Parameter Name=\"ReportingPeroid\" Type=\"DevExpress.DataAccess.Expression\">(System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)(GetDate(Iif(?ReportingPeroid = '2 Weeks', AddDays(Today(), -14), ?ReportingPeroid = '1 Month', AddMonths(Today(), -1), ?ReportingPeroid = '2 Months', AddMonths(Today(), -3), ?ReportingPeroid = '6 Months', AddMonths(Today(), -6), ?ReportingPeroid = '1 Year', AddYears(Today(), -1), ?ReportingPeroid = 'Since Go Live (9/25/2018)', GetDate('9/25/2018 12:00AM'), AddDays(Today(), -14)))\r\n)</Parameter><Sql>--DECLARE @REPORTINGPEROID DATE;\r\n--SET @REPORTINGPEROID = '1/20/2016';\r\n\r\nSELECT\r\n\tCTE.FullUserName\r\n\t, COUNT( DISTINCT OriginalFileID) AS InputDocumentCount\r\n\t, COUNT( DISTINCT CTE.DestFileID) AS OutputDocumentCount\r\n\t, CTE.InputDate\r\n\t, MAX(CurrentlyActive) AS CurrentlyActive\r\n\t, SUM( TotalMinutes) AS ActiveMinutes\r\n\t, CTE.Name\r\n\t, SUM( Pages) AS TotalPages\r\n\t, SUM(Correct) AS CorrectSum\r\n\t, SUM(Expected) AS ExpectedSum\r\n\t, SUM(Expected) - SUM(Correct) AS IncorrectPlusMissed\r\nFROM\r\n\t( -- This is necessary for getting the total minutes and pages. These values should only be applied once per doc, hence the rownum\r\n\tSELECT DISTINCT\r\n\t\tdbo.FAMUser.FullUserName\r\n\t\t, ReportingHIMStats.OriginalFileID\r\n\t\t, ReportingHIMStats.DestFileID\r\n\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t, vUsersWithActive.CurrentlyActive\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY TotalMinutes ORDER BY TotalMinutes ) = 1 THEN TotalMinutes\r\n\t\t\tELSE 0\r\n\t\t  END AS TotalMinutes\r\n\t\t, Workflow.Name\r\n\t\t, CASE \r\n\t\t\tWHEN ROW_NUMBER ( ) OVER (  PARTITION BY dbo.FAMFile.Pages, dbo.FAMFile.ID ORDER BY dbo.FAMFile.Pages desc ) = 1 THEN dbo.FAMFile.Pages\r\n\t\t\tELSE 0\r\n\t\t  END AS Pages\r\n\r\n\tFROM\r\n\t\tdbo.FAMUser\r\n\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.FAMFile\r\n\t\t\t\t\tON dbo.FAMFile.ID = ReportingHIMStats.OriginalFileID\r\n\r\n\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\tINNER JOIN dbo.vUsersWithActive \r\n\t\t\t\t\t\tON vUsersWithActive.FAMUserID = vFAMUserInputEventsTime.FAMUserID\r\n\r\n\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\tWHERE\r\n\t\tFAMUser.FullUserName IS NOT NULL\r\n\t) AS CTE\r\n\t\tLEFT OUTER JOIN ( --This subquery is necessary for reporing on the ReportingDataCaptureAccuracy. They require a different grouping than pages/active hours\r\n\t\t\t\t\t\t\tSELECT DISTINCT\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Correct) AS Correct\r\n\t\t\t\t\t\t\t\t, SUM(ReportingDataCaptureAccuracy.Expected) AS Expected\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\r\n\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\tdbo.FAMUser\r\n\t\t\t\t\t\t\t\t\tINNER JOIN dbo.ReportingHIMStats\r\n\t\t\t\t\t\t\t\t\t\tON FAMUser.ID = ReportingHIMStats.FAMUserID\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN (SELECT --Since the view is causing problems, I removed it entirly, so the date filter can be applied right away.\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE) AS [InputDate]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, SUM([FileTaskSession].[ActivityTime] / 60.0) AS TotalMinutes\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tFROM\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN [FileTaskSession] \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON[FAMSession].[ID] = [FileTaskSession].[FAMSessionID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND FileTaskSession.DateTimeStamp IS NOT NULL\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND [FileTaskSession].[DateTimeStamp] &gt;= @ReportingPeroid\r\n\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tINNER JOIN TaskClass \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tON FileTaskSession.TaskClassID = TaskClass.ID\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tAND\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t[TaskClass].GUID IN \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t'FD7867BD-815B-47B5-BAF4-243B8C44AABB'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '59496DF7-3951-49B7-B063-8C28F4CD843F'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'AD7F3F3F-20EC-4830-B014-EC118F6D4567'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, 'DF414AD2-742A-4ED7-AD20-C1A1C4993175'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t, '8ECBCC95-7371-459F-8A84-A2AFF7769800'\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t)\r\n\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t[FAMSession].[FAMUserID]\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t, CAST([FileTaskSession].[DateTimeStamp] AS DATE)\t\t\t\t\t\t\t\t\t\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t) AS vFAMUserInputEventsTime\r\n\t\t\t\t\t\t\t\t\t\t\tON vFAMUserInputEventsTime.FAMUserID = dbo.FAMUser.ID\r\n\t\t\t\t\t\t\t\t\t\t\tAND vFAMUserInputEventsTime.InputDate = ReportingHIMStats.DateProcessed\r\n\r\n\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.WorkflowFile \r\n\t\t\t\t\t\t\t\t\t\t\tON WorkflowFile.FileID = ReportingHIMStats.DestFileID\r\n  \r\n\t\t\t\t\t\t\t\t\t\t\tINNER JOIN dbo.Workflow \r\n\t\t\t\t\t\t\t\t\t\t\t\tON Workflow.ID = WorkflowFile.WorkflowID\r\n\t\t\t\t\t\t\t\t\t\t\t\tAND Workflow.Name &lt;&gt; N'zAdmin' \r\n\r\n\t\t\t\t\t\t\t\t\t\tLEFT OUTER JOIN dbo.ReportingDataCaptureAccuracy\r\n\t\t\t\t\t\t\t\t\t\t\tON ReportingDataCaptureAccuracy.FileID = ReportingHIMStats.DestFileID\r\n\r\n\t\t\t\t\t\t\tWHERE\r\n\t\t\t\t\t\t\t\tFAMUser.FullUserName IS NOT NULL\r\n\r\n\t\t\t\t\t\t\tGROUP BY\r\n\t\t\t\t\t\t\t\tdbo.FAMUser.FullUserName\r\n\t\t\t\t\t\t\t\t, ReportingHIMStats.DestFileID\r\n\t\t\t\t\t\t\t\t, vFAMUserInputEventsTime.InputDate\r\n\t\t\t\t\t\t\t\t, Workflow.Name\r\n\t\t\t\t\t\t\t) AS CTE2\r\n\t\t\t\t\t\t\t\tON CTE2.DestFileID = CTE.DestFileID\r\n\t\t\t\t\t\t\t\tAND CTE2.FullUserName = CTE.FullUserName\r\n\t\t\t\t\t\t\t\tAND CTE.InputDate = CTE2.InputDate\r\n\t\t\t\t\t\t\t\tAND CTE.Name = CTE2.Name\r\nGROUP BY\r\n\tCTE.FullUserName\r\n\t, CTE.InputDate\r\n\t, CTE.Name\r\n</Sql></Query><ResultSchema><DataSet Name=\"SQL Data Source 1\"><View Name=\"Query\"><Field Name=\"FullUserName\" Type=\"String\" /><Field Name=\"InputDocumentCount\" Type=\"Int32\" /><Field Name=\"OutputDocumentCount\" Type=\"Int32\" /><Field Name=\"InputDate\" Type=\"DateTime\" /><Field Name=\"CurrentlyActive\" Type=\"Int32\" /><Field Name=\"ActiveMinutes\" Type=\"Double\" /><Field Name=\"Name\" Type=\"String\" /><Field Name=\"TotalPages\" Type=\"Int32\" /><Field Name=\"CorrectSum\" Type=\"Int64\" /><Field Name=\"ExpectedSum\" Type=\"Int64\" /><Field Name=\"IncorrectPlusMissed\" Type=\"Int64\" /></View></DataSet></ResultSchema><ConnectionOptions CloseConnection=\"true\" DbCommandTimeout=\"0\" /><CalculatedFields><CalculatedField Name=\"Pages/Hour\" Expression=\"Sum([TotalPages]) / (SUM([ActiveMinutes]) / 60)\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"ActiveHours\" Expression=\"SUM([ActiveMinutes]) / 60\" DataType=\"Auto\" DataMember=\"Query\" /><CalculatedField Name=\"Documents/Hour\" Expression=\"SUM([OutputDocumentCount]) / [ActiveHours]\" DataType=\"Auto\" DataMember=\"Query\" /></CalculatedFields></SqlDataSource></DataSources><Parameters><Parameter Name=\"ReportingPeroid\" Value=\"2 Weeks\"><StaticListLookUpSettings><Values><Value>2 Weeks</Value><Value>1 Month</Value><Value>2 Months</Value><Value>6 Months</Value><Value>1 Year</Value><Value>Since Go Live (9/25/2018)</Value></Values></StaticListLookUpSettings></Parameter></Parameters><Items><Card ComponentName=\"cardDashboardItem1\" Name=\"Cards 1\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"InputDocumentCount\" Name=\"Input Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem2\" Name=\"Cards 2\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Documents\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"false\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem1\" Name=\"User Productivity\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Measure DataMember=\"CurrentlyActive\" Name=\"Active\" DefaultId=\"DataItem0\" /><Dimension DataMember=\"FullUserName\" DefaultId=\"DataItem1\" /><Measure DataMember=\"InputDocumentCount\" Name=\"Input Docs\" DefaultId=\"DataItem2\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"OutputDocumentCount\" Name=\"Output Docs\" DefaultId=\"DataItem3\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ActiveMinutes\" Name=\"Active Minutes\" DefaultId=\"DataItem4\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem5\" /><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem6\" /><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem7\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"ExpectedSum\" Name=\"Indexed Fields\" DefaultId=\"DataItem8\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"CorrectSum\" Name=\"Auto-Populated\" DefaultId=\"DataItem9\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure><Measure DataMember=\"IncorrectPlusMissed\" DefaultId=\"DataItem10\" /></DataItems><FormatRules><GridItemFormatRule Name=\"FormatRule 1\" DataItem=\"DataItem0\"><FormatConditionRangeSet ValueType=\"Number\"><RangeSet><Ranges><RangeInfo><Value Type=\"System.Decimal\" Value=\"0\" /><IconSettings IconType=\"ShapeRedCircle\" /></RangeInfo><RangeInfo><Value Type=\"System.Int32\" Value=\"1\" /><IconSettings IconType=\"ShapeGreenCircle\" /></RangeInfo></Ranges></RangeSet></FormatConditionRangeSet></GridItemFormatRule></FormatRules><GridColumns><GridMeasureColumn Weight=\"29.785894206549106\"><Measure DefaultId=\"DataItem0\" /></GridMeasureColumn><GridDimensionColumn Weight=\"90.050377833753117\"><Dimension DefaultId=\"DataItem1\" /></GridDimensionColumn><GridMeasureColumn Weight=\"83.816120906800975\"><Measure DefaultId=\"DataItem2\" /></GridMeasureColumn><GridMeasureColumn Weight=\"63.72795969773297\"><Measure DefaultId=\"DataItem3\" /></GridMeasureColumn><GridMeasureColumn Weight=\"78.967254408060413\"><Measure DefaultId=\"DataItem7\" /></GridMeasureColumn><GridMeasureColumn Weight=\"70.654911838790909\"><Measure DefaultId=\"DataItem4\" /></GridMeasureColumn><GridMeasureColumn Weight=\"95.591939546599463\"><Measure DefaultId=\"DataItem8\" /></GridMeasureColumn><GridMeasureColumn Weight=\"86.586901763224148\"><Measure DefaultId=\"DataItem9\" /></GridMeasureColumn><GridMeasureColumn Weight=\"60.9571788413098\"><Measure DefaultId=\"DataItem10\" /></GridMeasureColumn><GridMeasureColumn Weight=\"89.357682619647321\"><Measure DefaultId=\"DataItem6\" /></GridMeasureColumn><GridMeasureColumn Weight=\"75.503778337531443\"><Measure DefaultId=\"DataItem5\" /></GridMeasureColumn></GridColumns><GridOptions EnableBandedRows=\"true\" ColumnWidthMode=\"Manual\" /></Grid><RangeFilter ComponentName=\"rangeFilterDashboardItem1\" Name=\"Filter by Date\" ShowCaption=\"true\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Dimension DataMember=\"InputDate\" DateTimeGroupInterval=\"DayMonthYear\" DefaultId=\"DataItem0\" /><Measure DataMember=\"TotalPages\" DefaultId=\"DataItem1\" /></DataItems><Argument DefaultId=\"DataItem0\" /><Series><Simple SeriesType=\"Line\"><Value DefaultId=\"DataItem1\" /></Simple></Series></RangeFilter><Card ComponentName=\"cardDashboardItem3\" Name=\"Cards 3\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem4\" Name=\"Cards 4\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"ActiveHours\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem5\" Name=\"Cards 5\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"TotalPages\" Name=\"Pages\" DefaultId=\"DataItem0\"><NumericFormat FormatType=\"Number\" Precision=\"0\" Unit=\"Ones\" IncludeGroupSeparator=\"true\" /></Measure></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Card ComponentName=\"cardDashboardItem6\" Name=\"Cards 6\" ShowCaption=\"false\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><DataItems><Measure DataMember=\"Documents/Hour\" DefaultId=\"DataItem0\" /></DataItems><Card><ActualValue DefaultId=\"DataItem0\" /><AbsoluteVariationNumericFormat /><PercentVariationNumericFormat /><PercentOfTargetNumericFormat /><LayoutTemplate MinWidth=\"100\" MaxWidth=\"150\" Type=\"Lightweight\"><MainValue Visible=\"true\" ValueType=\"ActualValue\" DimensionIndex=\"0\" /><SubValue Visible=\"true\" ValueType=\"Title\" DimensionIndex=\"0\" /><BottomValue Visible=\"true\" ValueType=\"Subtitle\" DimensionIndex=\"0\" /><DeltaIndicator Visible=\"false\" /><Sparkline Visible=\"false\" /></LayoutTemplate></Card></Card><Grid ComponentName=\"gridDashboardItem2\" Name=\"By Queue\" DataSource=\"dashboardSqlDataSource1\" DataMember=\"Query\"><InteractivityOptions MasterFilterMode=\"Multiple\" /><DataItems><Dimension DataMember=\"Name\" DefaultId=\"DataItem0\" /><Measure DataMember=\"Pages/Hour\" DefaultId=\"DataItem1\" /></DataItems><GridColumns><GridDimensionColumn><Dimension DefaultId=\"DataItem0\" /></GridDimensionColumn><GridMeasureColumn><Measure DefaultId=\"DataItem1\" /></GridMeasureColumn></GridColumns><GridOptions /></Grid></Items><LayoutTree><LayoutGroup Orientation=\"Vertical\" Weight=\"100\"><LayoutGroup Weight=\"16.867469879518072\"><LayoutItem DashboardItem=\"cardDashboardItem1\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem2\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem5\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"19.277108433734941\"><LayoutItem DashboardItem=\"cardDashboardItem4\" Weight=\"30.371352785145888\" /><LayoutItem DashboardItem=\"cardDashboardItem6\" Weight=\"36.604774535809021\" /><LayoutItem DashboardItem=\"cardDashboardItem3\" Weight=\"33.023872679045091\" /></LayoutGroup><LayoutGroup Weight=\"29.036144578313252\"><LayoutItem DashboardItem=\"gridDashboardItem2\" Weight=\"19.761273209549071\" /><LayoutItem DashboardItem=\"gridDashboardItem1\" Weight=\"80.238726790450926\" /></LayoutGroup><LayoutItem DashboardItem=\"rangeFilterDashboardItem1\" Weight=\"34.819277108433738\" /></LayoutGroup></LayoutTree></Dashboard>",
                LastImportedDate = "2020-03-13T12:46:02.203",
                UseExtractedData = true,
                ExtractedDataDefinition = "<test />",
                DashboardGuid = Guid.Parse("2fe484cd-3e10-45a8-9c0c-1bcee4da4491"),
                UserName = "Trever_Gannon",
                FullUserName = "DataMaster"
            });

            DatabaseServices.Add(new DatabaseService() { 
                Description = "TestService",
                Settings = "{\r\n  \"$type\": \"Extract.Dashboard.ETL.DashboardExtractedDataService, Extract.Dashboard.ETL\",\r\n  \"Version\": 1,\r\n  \"Description\": \"TestService\",\r\n  \"Schedule\": {\r\n    \"$type\": \"Extract.Utilities.ScheduledEvent, Extract.Utilities\",\r\n    \"Exclusions\": [],\r\n    \"Version\": 1,\r\n    \"Start\": \"2020-03-12T15:56:59\",\r\n    \"End\": \"2020-03-12T15:56:59\",\r\n    \"RecurrenceUnit\": 2,\r\n    \"Duration\": null\r\n  }\r\n}",
                Enabled = true,
                Guid = Guid.Parse("78da4987-e380-4b2e-96df-90d5cc5a63ec")
            });

            DataEntryCounterDefinitions.Add(new DataEntryCounterDefinition() { 
                Name = "TestDataEntry",
                AttributeQuery = "SELECT WIN",
                RecordOnLoad = true,
                RecordOnSave = true,
                Guid = Guid.Parse("e76bffce-f546-4b61-8af7-a53f764a185e")
            });

            DBInfos.Add(new DBInfo() { Name = "ActionStatisticsUpdateFreqInSeconds", Value = "300" });
            DBInfos.Add(new DBInfo() { Name = "AllowDynamicTagCreation", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "AllowRestartableProcessing", Value = "2" });
            DBInfos.Add(new DBInfo() { Name = "AlternateComponentDataDir", Value = "C:\\TestDir" });
            DBInfos.Add(new DBInfo() { Name = "AttributeCollectionSchemaVersion", Value = "11" });
            DBInfos.Add(new DBInfo() { Name = "AutoCreateActions", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "AutoDeleteFileActionCommentOnComplete", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "AutoRevertNotifyEmailList", Value = "test@test.com" });
            DBInfos.Add(new DBInfo() { Name = "AutoRevertTimeOutInMinutes", Value = "5" });
            DBInfos.Add(new DBInfo() { Name = "CommandTimeout", Value = "120" });
            DBInfos.Add(new DBInfo() { Name = "ConnectionRetryTimeout", Value = "120" });
            DBInfos.Add(new DBInfo() { Name = "DatabaseID", Value = "7205a98a604343dc35659dd1b820033d287369feacb4cc3533469860611b300142056f888ef512f33d3f6ba98a43a107de4ccc87009b65c0caf9688947c9b43293ac8c4a540116125b5a2a19" });
            DBInfos.Add(new DBInfo() { Name = "DataEntrySchemaVersion", Value = "6" });
            DBInfos.Add(new DBInfo() { Name = "EmailEnableSettings", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "EmailPassword", Value = "" });
            DBInfos.Add(new DBInfo() { Name = "EmailPort", Value = "25" });
            DBInfos.Add(new DBInfo() { Name = "EmailPossibleInvalidSenderAddress", Value = "0" });
            DBInfos.Add(new DBInfo() { Name = "EmailPossibleInvalidServer", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "EmailSenderAddress", Value = "test@tester.com" });
            DBInfos.Add(new DBInfo() { Name = "EmailSenderName", Value = "TestSender" });
            DBInfos.Add(new DBInfo() { Name = "EmailServer", Value = "test@test.com" });
            DBInfos.Add(new DBInfo() { Name = "EmailSignature", Value = "TestSignature" });
            DBInfos.Add(new DBInfo() { Name = "EmailTimeout", Value = "0" });
            DBInfos.Add(new DBInfo() { Name = "EmailUsername", Value = "" });
            DBInfos.Add(new DBInfo() { Name = "EmailUseSSL", Value = "0" });
            DBInfos.Add(new DBInfo() { Name = "EnableDataEntryCounters", Value = "0" });
            DBInfos.Add(new DBInfo() { Name = "EnableLoadBalancing", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "ETLRestart", Value = "2020-03-12T15:50:31" });
            DBInfos.Add(new DBInfo() { Name = "FAMDBSchemaVersion", Value = "180" });
            DBInfos.Add(new DBInfo() { Name = "GetFilesToProcessTransactionTimeout", Value = "300" });
            DBInfos.Add(new DBInfo() { Name = "IDShieldSchemaVersion", Value = "6" });
            DBInfos.Add(new DBInfo() { Name = "InputActivityTimeout", Value = "30" });
            DBInfos.Add(new DBInfo() { Name = "LabDESchemaVersion", Value = "17" });
            DBInfos.Add(new DBInfo() { Name = "LastDBInfoChange", Value = "2020-03-13 12:42:31.393" });
            DBInfos.Add(new DBInfo() { Name = "LicenseContactEmail", Value = "" });
            DBInfos.Add(new DBInfo() { Name = "LicenseContactOrganization", Value = "" });
            DBInfos.Add(new DBInfo() { Name = "LicenseContactPhone", Value = "" });
            DBInfos.Add(new DBInfo() { Name = "NumberOfConnectionRetries", Value = "10" });
            DBInfos.Add(new DBInfo() { Name = "RequireAuthenticationBeforeRun", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "RequirePasswordToProcessAllSkippedFiles", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "RootPathForDashboardExtractedData", Value = "C:\\ProgramData\\Extract Systems\\Dashboards\\CachedData" });
            DBInfos.Add(new DBInfo() { Name = "SendAlertsToExtract", Value = "0" });
            DBInfos.Add(new DBInfo() { Name = "SendAlertsToSpecified", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "SkipAuthenticationForServiceOnMachines", Value = "TestMachine" });
            DBInfos.Add(new DBInfo() { Name = "SpecifiedAlertRecipients", Value = "test@test.com" });
            DBInfos.Add(new DBInfo() { Name = "StoreDBInfoChangeHistory", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "StoreDocNameChangeHistory", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "StoreDocTagHistory", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "StoreFAMSessionHistory", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "StoreFTPEventHistory", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "UpdateFileActionStateTransitionTable", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "UpdateQueueEventTable", Value = "1" });
            DBInfos.Add(new DBInfo() { Name = "MaxMillisecondsBetweenCheckForFilesToProcess", Value = "2000" });
            DBInfos.Add(new DBInfo() { Name = "MinMillisecondsBetweenCheckForFilesToProcess", Value = "2000" });

            FAMUsers.Add(new FAMUser() { UserName = "Trever_Gannon", FullUserName = "DataMaster"});
            FAMUsers.Add(new FAMUser() { UserName = "Reassign", FullUserName = "ToReassign" });

            FieldSearches.Add(new FieldSearch() { Enabled = true, FieldName = "FunField", AttributeQuery = "SELECT", Guid = Guid.Parse("78b081d6-6a7c-4d60-a3b0-1218f5e726f4") });

            FileHandlers.Add(new FileHandler() {
                Enabled = true,
                AppName = "FunApp",
                IconPath = "C:\\Wut",
                ApplicationPath = "C:\\Why",
                Arguments = "Turtle",
                AdminOnly = true,
                AllowMultipleFiles = true,
                SupportsErrorHandling = true,
                Blocking = true,
                WorkflowName = "Test Workflow",
                Guid = Guid.Parse("26931eb8-814e-4821-b13e-4d32765df237")
            });

            LabDEEncounters.Add(new LabDEEncounter() {
                CSN = "1000006096",
                PatientMRN = "10000001",
                EncounterDateTime = "2012-01-06T00:00:00",
                Department = "LK FP",
                EncounterType = "ANCILLARY",
                EncounterProvider = "LAKESIDE ANCILLARY",
                DischargeDate = "2013-01-06T00:00:00",
                AdmissionDate = "2014-01-06T00:00:00",
                ADTMessage = "<ADT_A01><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1>EPIC</HD.1></MSH.3><MSH.5><HD.1>EXTRACT</HD.1></MSH.5><MSH.7><TS.1>20190529</TS.1></MSH.7><MSH.8>23221</MSH.8><MSH.9><MSG.1>ADT</MSG.1><MSG.2>A08</MSG.2></MSH.9><MSH.10>396019498</MSH.10><MSH.11><PT.1>P</PT.1></MSH.11></MSH><PV1><PV1.3><PL.1>LK FP</PL.1><PL.4><HD.1>LK</HD.1></PL.4></PV1.3><PV1.7><XCN.1>740044</XCN.1><XCN.2><FN.1>ANCILLARY</FN.1></XCN.2><XCN.3>LAKESIDE</XCN.3></PV1.7><PV1.19><CX.1>1000006096</CX.1></PV1.19><PV1.44><TS.1>20120106</TS.1></PV1.44></PV1></ADT_A01>",
                Guid = Guid.Parse("e8c1faed-8af1-4521-a83e-efceaac348ec")
            });

            LabDEEncounters.Add(new LabDEEncounter()
            {
                CSN = "234234",
                PatientMRN = "10000001",
                EncounterDateTime = "2016-01-06T00:00:00",
                Department = "LK F7",
                EncounterType = "hgff",
                EncounterProvider = "LAKESIDE ANCILLARY",
                DischargeDate = "2018-01-06T00:00:00",
                AdmissionDate = "2013-01-06T00:00:00",
                ADTMessage = "<ADT_A01><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1>EPIC</HD.1></MSH.3><MSH.5><HD.1>EXTRACT</HD.1></MSH.5><MSH.7><TS.1>20190529</TS.1></MSH.7><MSH.8>23221</MSH.8><MSH.9><MSG.1>ADT</MSG.1><MSG.2>A08</MSG.2></MSH.9><MSH.10>396019498</MSH.10><MSH.11><PT.1>P</PT.1></MSH.11></MSH><PV1><PV1.3><PL.1>LK FP</PL.1><PL.4><HD.1>LK</HD.1></PL.4></PV1.3><PV1.7><XCN.1>740044</XCN.1><XCN.2><FN.1>ANCILLARY</FN.1></XCN.2><XCN.3>LAKESIDE</XCN.3></PV1.7><PV1.19><CX.1>1000006096</CX.1></PV1.19><PV1.44><TS.1>20120106</TS.1></PV1.44></PV1></ADT_A01>",
                Guid = Guid.Parse("cafb7a01-ad3b-4d74-b9a8-238e6bc7f08c")
            });

            LabDEOrders.Add(new LabDEOrder() {
                OrderNumber = "100014135",
                OrderCode = "90686",
                PatientMRN = "10000001",
                ReceivedDateTime = "2018-10-02T15:22:00.53",
                OrderStatus = "A",
                ReferenceDateTime = "2018-10-02T00:00:00",
                ORMMessage = "<ORM_O01><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1>EPIC</HD.1></MSH.3><MSH.5><HD.1>EXTRACT</HD.1></MSH.5><MSH.7><TS.1>20181002152150</TS.1></MSH.7><MSH.9><MSG.1>ORM</MSG.1><MSG.2>O01</MSG.2></MSH.9><MSH.10>7124912</MSH.10><MSH.12><VID.1>2.3</VID.1></MSH.12></MSH><ORM_O01.PATIENT><ORM_O01.PATIENT_VISIT><PV1><PV1.3><PL.1>WAC WI</PL.1><PL.4><HD.1>WACL</HD.1></PL.4></PV1.3><PV1.7><XCN.1>741102</XCN.1><XCN.2><FN.1>ANCILLARY</FN.1></XCN.2><XCN.3>WACL</XCN.3></PV1.7><PV1.19><CX.1>1173770646</CX.1></PV1.19><PV1.44><TS.1>20181002145829</TS.1></PV1.44></PV1></ORM_O01.PATIENT_VISIT></ORM_O01.PATIENT><ORM_O01.ORDER><ORC><ORC.1>NW</ORC.1><ORC.2><EI.1>100014135</EI.1></ORC.2><ORC.9><TS.1>20181002152148</TS.1></ORC.9><ORC.10><XCN.1>80836</XCN.1><XCN.2><FN.1>STICKLER</FN.1></XCN.2><XCN.3>KATHRYN</XCN.3></ORC.10><ORC.12><XCN.1>30069</XCN.1><XCN.2><FN.1>LUNDE</FN.1></XCN.2><XCN.3>LARA</XCN.3><XCN.4>N</XCN.4></ORC.12><ORC.13><PL.1>1030100140037</PL.1><PL.4><HD.1>103010014</HD.1></PL.4></ORC.13></ORC><ORM_O01.ORDER_DETAIL><ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP><OBR><OBR.1>1</OBR.1><OBR.2><EI.1>100014135</EI.1></OBR.2><OBR.4><CE.1>90686</CE.1><CE.2>FLU VACCINE,QUAD, NO PRESERV,IM  .5 ML</CE.2></OBR.4><OBR.6><TS.1>20181002</TS.1></OBR.6><OBR.16><XCN.1>30069</XCN.1><XCN.2><FN.1>LUNDE</FN.1></XCN.2><XCN.3>LARA</XCN.3><XCN.4>N</XCN.4></OBR.16></OBR></ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP></ORM_O01.ORDER_DETAIL></ORM_O01.ORDER></ORM_O01>",
                EncounterID = "1000006096",
                AccessionNumber = "6666",
                Guid = Guid.Parse("b0fdeafb-dc1f-49dd-ae66-997dca7bccc2")
            });

            LabDEPatients.Add(new LabDEPatient() { 
                MRN = "10000001",
                FirstName = "John",
                MiddleName = "P",
                LastName = "Doe",
                Suffix = "JR",
                DOB = "1993-04-08T00:00:00",
                Gender = "F",
                MergedInto = null,
                CurrentMRN = "10000001",
                Guid = Guid.Parse("751ea05a-a0a2-47bd-b5f3-09e78a565e7b")
            });

            LabDEPatients.Add(new LabDEPatient()
            {
                MRN = "10000002",
                FirstName = "Jane",
                MiddleName = "L",
                LastName = "Doen",
                Suffix = "JR",
                DOB = "1996-04-08T00:00:00",
                Gender = "M",
                MergedInto = "10000001",
                CurrentMRN = "10000002",
                Guid = Guid.Parse("85aaf641-a424-431b-8300-19d89a474b42")
            });

            LabDEProviders.Add(new LabDEProvider() {
                ID = "100",
                FirstName = "Joel",
                MiddleName = "T M",
                LastName = "BamFord",
                ProviderType = "PHYSICIAN",
                Title = "MDS",
                Degree = "MD",
                Departments = "DC DER;AS DER;IF DER;HI DER",
                Specialties = "DERM",
                Phone = "(218)786-3434",
                Fax = "(218)786-3066",
                Address = "400 EAST THIRD STREET, DULUTH, MN, 55805",
                OtherProviderID = "1326081662",
                Inactive = true,
                MFNMessage = "<MFN_M02><MSH><MSH.1>|</MSH.1><MSH.2>^~\\&amp;</MSH.2><MSH.3><HD.1 /><HD.2 /><HD.3 /></MSH.3><MSH.4><HD.1>EPIC</HD.1><HD.2 /><HD.3 /></MSH.4><MSH.5><HD.1 /><HD.2 /><HD.3 /></MSH.5><MSH.6><HD.1 /><HD.2 /><HD.3 /></MSH.6><MSH.7><TS.1>20190305170108</TS.1><TS.2 /></MSH.7><MSH.8>872170</MSH.8><MSH.9><MSG.1>MFN</MSG.1><MSG.2>M02</MSG.2><MSG.3 /></MSH.9><MSH.10>776567</MSH.10><MSH.11><PT.1>P</PT.1><PT.2 /></MSH.11></MSH><MFI><MFI.1><CE.1>SER</CE.1></MFI.1><MFI.3>UPD</MFI.3><MFI.6>NE</MFI.6></MFI><MFN_M02.MF_STAFF><MFE><MFE.1>MDL</MFE.1><MFE.4>100</MFE.4></MFE><STF><STF.1><CE.1>100</CE.1><CE.2 /><CE.3>DENTP</CE.3></STF.1><STF.3><XPN.1><FN.1>BAMFORD</FN.1></XPN.1><XPN.2>JOEL</XPN.2><XPN.3>T M</XPN.3><XPN.5>MD</XPN.5><XPN.6>MD</XPN.6></STF.3><STF.4>PERSON</STF.4><STF.5>M</STF.5><STF.6><TS.1 /></STF.6><STF.7>I</STF.7><STF.8><CE.1>DC DER</CE.1></STF.8><STF.10><XTN.1>(218)786-3434</XTN.1><XTN.4 /></STF.10><STF.11><XAD.1><SAD.1>400 EAST THIRD STREET</SAD.1></XAD.1><XAD.3>DULUTH</XAD.3><XAD.4>MN</XAD.4><XAD.5>55805</XAD.5></STF.11></STF><PRA><PRA.1><CE.1>100</CE.1><CE.2 /><CE.3>DENTP</CE.3></PRA.1><PRA.2><CE.1 /></PRA.2><PRA.3>PHYSICIAN</PRA.3><PRA.4>Y</PRA.4><PRA.5><SPD.1>Derm</SPD.1></PRA.5><PRA.6><PLN.1>D80218</PLN.1><PLN.2 /><PLN.3 /><PLN.4 /></PRA.6><PRA.8 /><PRA.9><CE.1 /><CE.2 /><CE.3 /></PRA.9><PRA.10 /></PRA></MFN_M02.MF_STAFF></MFN_M02>",
                Guid = Guid.Parse("94c0238c-3e42-42df-8eef-4b95651bbc5f")
            });

            Logins.Add(new Login() { UserName = "admin", Password = "e086da2321be72f0525b25d5d5b0c6d7", Guid = Guid.Parse("e507f885-2773-4ea8-9d44-f3f21dd1df76") });
            Logins.Add(new Login() { UserName = "TestUser", Password = "3bddc2b8b6f3a9eb32e586d18212ca4c", Guid = Guid.Parse("f5ef7d29-f399-4e46-94f2-ef2551fe415d") });

            MetadataFields.Add(new MetadataField() { Name = "TestMetadataField", Guid = Guid.Parse("1cffbe35-a3b8-4ded-b45d-73109085760b") });
            MetadataFields.Add(new MetadataField() { Name = "Reassign", Guid = Guid.Parse("3cd22248-9e0f-41f9-8754-b4d0a9bc087d") });

            MLModels.Add(new MLModel() { Name = "22", Guid = Guid.Parse("77b12d3a-8574-46c2-846b-e136025ae4be") });

            Tags.Add(new Tag() { TagName = "TestTag", TagDescription = "TestTagDescription", Guid = Guid.Parse("b64ffa00-6f1a-4239-803c-f975a1c59f2f") });

            UserCreatedCounters.Add(new UserCreatedCounter() { CounterName = "TestCounter", Value = "1337", Guid = Guid.Parse("c526a990-67e3-4bea-a513-6bfd81e76187") });

            WebAppConfigurations.Add(new WebAppConfig() {
                Type = "RedactionVerificationSettings",
                Settings = "{\"DocumentTypes\":\"C:\\\\TestAvail\",\"InactivityTimeout\":5,\"RedactionTypes\":[\"Test\"]}",
                WebAppConfigGuid = Guid.Parse("befdb629-af55-4bb2-8309-28559a42ae36"),
                WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520")
            });

            Workflows.Add(new Workflow() { 
                WorkflowGuid = Guid.Parse("0e7193e0-7416-47b3-b5fe-24e26fdf6520"),
                Name = "Test Workflow",
                WorkflowTypeCode = "R",
                Description = "Test Description Workflow",
                DocumentFolder = "C:\\TestFolderWorkflow",
                OutputFilePathInitializationFunction = "c:\\TestFun",
                LoadBalanceWeight = 2,
                MetadataFieldGuid = Guid.Parse("1cffbe35-a3b8-4ded-b45d-73109085760b"),
                EditActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                EndActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                PostEditActionGuid = Guid.Parse("cd27650d-dfe4-44c3-9fc8-96ea99f7a4e2"),
                PostWorkflowActionGuid = Guid.Parse("cd27650d-dfe4-44c3-9fc8-96ea99f7a4e2"),
                StartActionGuid = Guid.Parse("04f0e473-5714-4687-a147-8b7fb6f5335e"),
                AttributeSetNameGuid = Guid.Parse("7c081610-2f63-4f0c-9a3b-d018176bd5ea"),
            });

            Workflows.Add(new Workflow()
            {
                WorkflowGuid = Guid.Parse("0a3e4811-f55f-49d2-9c6f-0b64cd56961e"),
                Name = "Reassign",
                WorkflowTypeCode = "R",
                Description = "Tests2 Description Workflow",
                LoadBalanceWeight = 4,
            });
        }
    }
}
