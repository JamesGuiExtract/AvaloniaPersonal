<Package Version="1.0" Date="03#sl#11#sl#2015#sp#14:12:54#sp#Central#sp#Daylight#sp#Time" Desc="" User="steve_kurth" Machine="HAWKEYE"><Object Type="Correlation" Name="#sl#Abnormal#sp#Flags" ModDateUtc="2015-03-11T19:09:40.295Z" ModDate="2015#sl#03#sl#11#sp#14:09:40" Length="1178">&lt;Correlation Desc="" Embedded="0" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Groups&gt;&lt;CodesetGroup Domain="1" Unique="1" Visible="1"&gt;&lt;Codesets&gt;&lt;Codeset Name="#sl#Extract#sp#Systems#sl#Extract#sp#Abnormal#sp#Flags" DescVisible="False" /&gt;&lt;/Codesets&gt;&lt;/CodesetGroup&gt;&lt;CodesetGroup Domain="0" Unique="1" Visible="1"&gt;&lt;Codesets&gt;&lt;Codeset Name="#sl#Extract#sp#Systems#sl#HIS#sp#Abnormal#sp#Flags" DescVisible="False" /&gt;&lt;/Codesets&gt;&lt;/CodesetGroup&gt;&lt;/Groups&gt;&lt;Primaries /&gt;&lt;Rows&gt;&lt;Array&gt;&lt;Element Value="L" /&gt;&lt;Element Value="L" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="H" /&gt;&lt;Element Value="H" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="A" /&gt;&lt;Element Value="A" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="N" /&gt;&lt;Element Value="N" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="S" /&gt;&lt;Element Value="S" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="I" /&gt;&lt;Element Value="I" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="LL" /&gt;&lt;Element Value="LL" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="HH" /&gt;&lt;Element Value="HH" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="AA" /&gt;&lt;Element Value="AA" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;/Rows&gt;&lt;/Correlation&gt;</Object><Object Type="Notification" Name="#sl#Alerting#sl#Alert#sp#Receipents" ModDateUtc="2014-01-16T23:26:46Z" ModDate="2014#sl#01#sl#16#sp#17:26:46" Length="326">&lt;SmtpNotification description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Recipients&gt;;cr;;lf;    &lt;Recipient name="Extract Systems Support" /&gt;;cr;;lf;    &lt;Recipient name="UW Health Stefano Walczyk" /&gt;;cr;;lf;    &lt;Recipient name="UW Health Kris Shultz" /&gt;;cr;;lf;  &lt;/Recipients&gt;;cr;;lf;  &lt;Server name="E-mail Server" /&gt;;cr;;lf;&lt;/SmtpNotification&gt;</Object><Object Type="Alert#sp#Config" Name="#sl#Alerting#sl#Alerting" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="228">&lt;AlertConf description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;DaySet_AlertSchedule_NameMap&gt;;cr;;lf;    &lt;MapElement dayset="Every Day" alertschedule="Every Day" /&gt;;cr;;lf;  &lt;/DaySet_AlertSchedule_NameMap&gt;;cr;;lf;&lt;/AlertConf&gt;</Object><Object Type="Time#sp#Period" Name="#sl#Alerting#sl#All#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="180">&lt;TimePeriod description="Generated by the Alert Wizard on 7/17/2013" version="1" color="16711680"&gt;;cr;;lf;  &lt;TimeRanges&gt;;cr;;lf;    &lt;TimeRange name="All Day" /&gt;;cr;;lf;  &lt;/TimeRanges&gt;;cr;;lf;&lt;/TimePeriod&gt;</Object><Object Type="Time#sp#Range" Name="#sl#Alerting#sl#All#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="167">&lt;TimeRange description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;StartTime hour="0" minute="0" /&gt;;cr;;lf;  &lt;EndTime hour="24" minute="0" /&gt;;cr;;lf;&lt;/TimeRange&gt;</Object><Object Type="Action#sp#List" Name="Convert#sp#Date" ModDateUtc="2012-07-19T16:27:23Z" ModDate="2012#sl#07#sl#19#sp#11:27:23" Length="4114">&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Load" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ObjParms Message="Unparsed" Derivative="Unparsed" Var="in" Position="1" MessageType="2" Check="0" Opt="112" ErrorAct="0" MsgOpt="0" ExtraContent="0" Validate="0" RecursionDepth="0" Source="0" MMHandle="" RelatedMessageKey=""&gt;&lt;ArgumentList /&gt;&lt;/ObjParms&gt;&lt;/Operator&gt;&lt;Operator Type="Try" CompatibleVersion="4.0.0" Version="1"&gt;&lt;TryList Exceptions="8" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Date" Desc="" Disabled="0" UseDotNetDate="0"&gt;&lt;ObjParms Format="yyyymmdd" SourceFormat="mm#sl#dd#sl#yyyy" CompatibleVersion="4.0.0" Version="2" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;DotNetFormat /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/TryList&gt;&lt;CatchList TypeVar="ErrorType" NumVar="ErrorNum" StringVar="ErrorString" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Try" CompatibleVersion="4.0.0" Version="1"&gt;&lt;TryList Exceptions="8" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Date" Desc="" Disabled="0" UseDotNetDate="0"&gt;&lt;ObjParms Format="yyyymmdd" SourceFormat="m#sl#dd#sl#yyyy" CompatibleVersion="4.0.0" Version="2" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;DotNetFormat /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/TryList&gt;&lt;CatchList TypeVar="ErrorType" NumVar="ErrorNum" StringVar="ErrorString" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Try" CompatibleVersion="4.0.0" Version="1"&gt;&lt;TryList Exceptions="8" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Date" Desc="" Disabled="0" UseDotNetDate="0"&gt;&lt;ObjParms Format="yyyymmdd" SourceFormat="mm#sl#d#sl#yyyy" CompatibleVersion="4.0.0" Version="2" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;DotNetFormat /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/TryList&gt;&lt;CatchList TypeVar="ErrorType" NumVar="ErrorNum" StringVar="ErrorString" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Copy" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;Operand Type="Literal" Value="" EscapeBackslashes="1" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/CatchList&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/CatchList&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/CatchList&gt;&lt;/Operator&gt;&lt;Operator Type="Pass" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0" Message="in" Check="0" Opt="112" ErrorAct="0" Validate="0" Dest="0" MMHandle=""&gt;&lt;ArgumentList /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;</Object><Object Type="Action#sp#List" Name="Convert#sp#Time" ModDateUtc="2012-07-19T16:27:23Z" ModDate="2012#sl#07#sl#19#sp#11:27:23" Length="3044">&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Load" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ObjParms Message="Unparsed" Derivative="Unparsed" Var="in" Position="1" MessageType="2" Check="0" Opt="112" ErrorAct="0" MsgOpt="0" ExtraContent="0" Validate="0" RecursionDepth="0" Source="0" MMHandle="" RelatedMessageKey=""&gt;&lt;ArgumentList /&gt;&lt;/ObjParms&gt;&lt;/Operator&gt;&lt;Operator Type="Try" CompatibleVersion="4.0.0" Version="1"&gt;&lt;TryList Exceptions="8" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Date" Desc="" Disabled="0" UseDotNetDate="0"&gt;&lt;ObjParms Format="hhnn" SourceFormat="hh:nn" CompatibleVersion="4.0.0" Version="2" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;DotNetFormat /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/TryList&gt;&lt;CatchList TypeVar="ErrorType" NumVar="ErrorNum" StringVar="ErrorString" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Try" CompatibleVersion="4.0.0" Version="1"&gt;&lt;TryList Exceptions="8" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Date" Desc="" Disabled="0" UseDotNetDate="0"&gt;&lt;ObjParms Format="hhnn" SourceFormat="h:nn" CompatibleVersion="4.0.0" Version="2" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;DotNetFormat /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/TryList&gt;&lt;CatchList TypeVar="ErrorType" NumVar="ErrorNum" StringVar="ErrorString" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;ActionList CompatibleVersion="4.0.0" Version="1" Desc=""&gt;&lt;Operator Type="Copy" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0"&gt;&lt;Operand Type="Literal" Value="" EscapeBackslashes="1" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;Operand Type="Path" Var="in" Path="#sl#data" Derivative="Unparsed" Display="data" Message="Unparsed" MessageType="2" Semantics="0" MsgOpt="0" RecursionDepth="0" CompatibleVersion="4.0.0" Version="5" /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/CatchList&gt;&lt;/Operator&gt;&lt;/ActionList&gt;&lt;/CatchList&gt;&lt;/Operator&gt;&lt;Operator Type="Pass" Desc="" CompatibleVersion="4.0.0" Version="1" Disabled="0" Message="in" Check="0" Opt="112" ErrorAct="0" Validate="0" Dest="0" MMHandle=""&gt;&lt;ArgumentList /&gt;&lt;/Operator&gt;&lt;/ActionList&gt;</Object><Object Type="Mail#sp#Server" Name="#sl#Alerting#sl#E-mail#sp#Server" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="259">&lt;SmtpServer description="Generated by the Alert Wizard on 7/17/2013" version="3" from="test-labde@uwhealth.org" acceptSSLCerts="0" attemptSSL="0"&gt;;cr;;lf;  &lt;ConnectInfo name="mail.hosp.wisc.edu" port="25" /&gt;;cr;;lf;  &lt;Credentials username="" password="" /&gt;;cr;;lf;&lt;/SmtpServer&gt;</Object><Object Type="Threshold" Name="#sl#Alerting#sl#Error#sp#Handler" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="322">&lt;Threshold version="2" description="Generated by the Alert Wizard on 7/17/2013" requiresresolution="-1"&gt;;cr;;lf;  &lt;AlertLevel name="" /&gt;;cr;;lf;  &lt;Notification name="Alert Receipents" /&gt;;cr;;lf;  &lt;Actions&gt;;cr;;lf;    &lt;SendEmail notification="Alert Receipents" version="1"&gt;;cr;;lf;      &lt;Body replace="0" /&gt;;cr;;lf;    &lt;/SendEmail&gt;;cr;;lf;  &lt;/Actions&gt;;cr;;lf;&lt;/Threshold&gt;</Object><Object Type="Alert#sp#Schedule" Name="#sl#Alerting#sl#Every#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="239">&lt;AlertSchedule description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;TimePeriod_AlertSet_NameMap&gt;;cr;;lf;    &lt;MapElement timeperiod="All Day" alertset="Every Day-All Day" /&gt;;cr;;lf;  &lt;/TimePeriod_AlertSet_NameMap&gt;;cr;;lf;&lt;/AlertSchedule&gt;</Object><Object Type="Day#sp#Set" Name="#sl#Alerting#sl#Every#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="179">&lt;DaySet description="Generated by the Alert Wizard on 7/17/2013" version="1" color="8454143"&gt;;cr;;lf;  &lt;MatchingDays&gt;;cr;;lf;    &lt;MatchingDay name="Every Day" /&gt;;cr;;lf;  &lt;/MatchingDays&gt;;cr;;lf;&lt;/DaySet&gt;</Object><Object Type="Matching#sp#Days" Name="#sl#Alerting#sl#Every#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="230">&lt;DailyMatchingDay description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;StartDate useDate="0" month="12" day="30" year="1899" /&gt;;cr;;lf;  &lt;EndDate hasEndDate="0" month="7" day="17" year="2013" /&gt;;cr;;lf;&lt;/DailyMatchingDay&gt;</Object><Object Type="Alert#sp#Set" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="429">&lt;AlertSet description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Alerts&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-Errored Message" /&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-No Response" /&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-Not Connected" /&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-Queue Depth" /&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-Send Fail" /&gt;;cr;;lf;    &lt;Alert name="Every Day-All Day-Stopped" /&gt;;cr;;lf;  &lt;/Alerts&gt;;cr;;lf;&lt;/AlertSet&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-Errored#sp#Message" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="187">&lt;MessageErroredAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="Error Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;&lt;/MessageErroredAlert&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-No#sp#Response" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="191">&lt;AwaitingReplyAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="No Response Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;&lt;/AwaitingReplyAlert&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-Not#sp#Connected" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="191">&lt;NotConnectedAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="Not Connected Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;&lt;/NotConnectedAlert&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-Queue#sp#Depth" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="238">&lt;QueueDepthAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="Queue Depth Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;  &lt;QueueInfo direction="inbound" position="both" /&gt;;cr;;lf;&lt;/QueueDepthAlert&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-Send#sp#Fail" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="193">&lt;MessageSendFailedAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="Error Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;&lt;/MessageSendFailedAlert&gt;</Object><Object Type="Alert" Name="#sl#Alerting#sl#Every#sp#Day-All#sp#Day-Stopped" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="175">&lt;StoppedAlert description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;Thresholds&gt;;cr;;lf;    &lt;Threshold name="Stopped Handler" /&gt;;cr;;lf;  &lt;/Thresholds&gt;;cr;;lf;&lt;/StoppedAlert&gt;</Object><Object Type="Code#sp#Set" Name="#sl#Extract#sp#Systems#sl#Extract#sp#Abnormal#sp#Flags" ModDateUtc="2015-03-11T19:08:48.139Z" ModDate="2015#sl#03#sl#11#sp#14:08:48" Length="597">&lt;Codeset Desc="" Color="16777215" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Elements&gt;&lt;Element Code="L" Desc="Abnormally#sp#low" /&gt;&lt;Element Code="LL" Desc="Critically#sp#low" /&gt;&lt;Element Code="H" Desc="Abnormally#sp#high" /&gt;&lt;Element Code="HH" Desc="Critically#sp#high" /&gt;&lt;Element Code="A" Desc="abnormal,#sp#non-numeric" /&gt;&lt;Element Code="N" Desc="Normal,#sp#non-numeric" /&gt;&lt;Element Code="S" Desc="Sensitive#sp#antibiotic#sp#susceptibility" /&gt;&lt;Element Code="I" Desc="Intermediate#sp#antibiotic#sp#susceptibility" /&gt;&lt;Element Code="AA" Desc="Critically#sp#abnormal,#sp#non-numeric" /&gt;&lt;/Elements&gt;&lt;/Codeset&gt;</Object><Object Type="Code#sp#Set" Name="#sl#Extract#sp#Systems#sl#Extract#sp#Result#sp#Status" ModDateUtc="2013-06-06T15:52:45Z" ModDate="2013#sl#06#sl#06#sp#10:52:45" Length="271">&lt;Codeset Desc="" Color="16777215" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Elements&gt;&lt;Element Code="Final" Desc="Final" /&gt;&lt;Element Code="Preliminary" Desc="Preliminary" /&gt;&lt;Element Code="Edited" Desc="Edited" /&gt;&lt;Element Code="Reviewed" Desc="Reviewed" /&gt;&lt;/Elements&gt;&lt;/Codeset&gt;</Object><Object Type="Recipient" Name="#sl#Alerting#sl#Extract#sp#Systems#sp#Support" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="161">&lt;EmailRecipient description="Generated by the Alert Wizard on 7/17/2013" version="1"&gt;;cr;;lf;  &lt;EmailAddress address="support@extractsystems.com" /&gt;;cr;;lf;&lt;/EmailRecipient&gt;</Object><Object Type="Code#sp#Set" Name="#sl#Extract#sp#Systems#sl#HIS#sp#Abnormal#sp#Flags" ModDateUtc="2015-03-11T19:09:23.112Z" ModDate="2015#sl#03#sl#11#sp#14:09:23" Length="597">&lt;Codeset Desc="" Color="16777215" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Elements&gt;&lt;Element Code="L" Desc="Abnormally#sp#low" /&gt;&lt;Element Code="LL" Desc="Critically#sp#low" /&gt;&lt;Element Code="H" Desc="Abnormally#sp#high" /&gt;&lt;Element Code="HH" Desc="Critically#sp#high" /&gt;&lt;Element Code="A" Desc="abnormal,#sp#non-numeric" /&gt;&lt;Element Code="N" Desc="Normal,#sp#non-numeric" /&gt;&lt;Element Code="S" Desc="Sensitive#sp#antibiotic#sp#susceptibility" /&gt;&lt;Element Code="I" Desc="Intermediate#sp#antibiotic#sp#susceptibility" /&gt;&lt;Element Code="AA" Desc="Critically#sp#abnormal,#sp#non-numeric" /&gt;&lt;/Elements&gt;&lt;/Codeset&gt;</Object><Object Type="Code#sp#Set" Name="#sl#Extract#sp#Systems#sl#HIS#sp#Result#sp#Status" ModDateUtc="2013-05-30T17:25:32Z" ModDate="2013#sl#05#sl#30#sp#12:25:32" Length="248">&lt;Codeset Desc="" Color="16777215" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Elements&gt;&lt;Element Code="F" Desc="Final" /&gt;&lt;Element Code="P" Desc="Preliminary" /&gt;&lt;Element Code="C" Desc="Corrected" /&gt;&lt;Element Code="?" Desc="Reviewed" /&gt;&lt;/Elements&gt;&lt;/Codeset&gt;</Object><Object Type="Threshold" Name="#sl#Alerting#sl#No#sp#Response#sp#Handler" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="356">&lt;TimeThreshold version="2" description="Generated by the Alert Wizard on 7/17/2013" requiresresolution="0"&gt;;cr;;lf;  &lt;Limits seconds="60" /&gt;;cr;;lf;  &lt;AlertLevel name="" /&gt;;cr;;lf;  &lt;Notification name="Alert Receipents" /&gt;;cr;;lf;  &lt;Actions&gt;;cr;;lf;    &lt;SendEmail notification="Alert Receipents" version="1"&gt;;cr;;lf;      &lt;Body replace="0" /&gt;;cr;;lf;    &lt;/SendEmail&gt;;cr;;lf;  &lt;/Actions&gt;;cr;;lf;&lt;/TimeThreshold&gt;</Object><Object Type="Threshold" Name="#sl#Alerting#sl#Not#sp#Connected#sp#Handler" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="411">&lt;TimeThreshold version="2" description="Generated by the Alert Wizard on 7/17/2013" requiresresolution="0"&gt;;cr;;lf;  &lt;Limits seconds="900" /&gt;;cr;;lf;  &lt;AlertLevel name="" /&gt;;cr;;lf;  &lt;Notification name="Alert Receipents" /&gt;;cr;;lf;  &lt;Actions&gt;;cr;;lf;    &lt;SendEmail notification="Alert Receipents" version="1"&gt;;cr;;lf;      &lt;Body replace="0" /&gt;;cr;;lf;    &lt;/SendEmail&gt;;cr;;lf;    &lt;ControlConnection mode="restart" version="1" /&gt;;cr;;lf;  &lt;/Actions&gt;;cr;;lf;&lt;/TimeThreshold&gt;</Object><Object Type="Connection" Name="OB_UW_ORU" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="6119">&lt;Connection Version="8" CompatibleVersion="4.2.0"&gt;;cr;;lf;  &lt;Description&gt;&lt;/Description&gt;;cr;;lf;  &lt;ConnectionType&gt;1&lt;/ConnectionType&gt;;cr;;lf;  &lt;Subtype&gt;None&lt;/Subtype&gt;;cr;;lf;  &lt;AwaitReply&gt;True&lt;/AwaitReply&gt;;cr;;lf;  &lt;CheckMSA1&gt;False&lt;/CheckMSA1&gt;;cr;;lf;  &lt;ExpectHL7&gt;True&lt;/ExpectHL7&gt;;cr;;lf;  &lt;HoldQueue&gt;False&lt;/HoldQueue&gt;;cr;;lf;  &lt;IsTcpIpServer&gt;False&lt;/IsTcpIpServer&gt;;cr;;lf;  &lt;MatchMSA2&gt;False&lt;/MatchMSA2&gt;;cr;;lf;  &lt;OmitHL7AckTrigger&gt;False&lt;/OmitHL7AckTrigger&gt;;cr;;lf;  &lt;ReplyTimeoutIsInfinite&gt;True&lt;/ReplyTimeoutIsInfinite&gt;;cr;;lf;  &lt;RequireHL7ACK&gt;False&lt;/RequireHL7ACK&gt;;cr;;lf;  &lt;RequireHL7Reply&gt;True&lt;/RequireHL7Reply&gt;;cr;;lf;  &lt;RequireMSH10Out&gt;False&lt;/RequireMSH10Out&gt;;cr;;lf;  &lt;ResendIfMSA2Mismatch&gt;True&lt;/ResendIfMSA2Mismatch&gt;;cr;;lf;  &lt;ResendIfReplyNotHL7&gt;True&lt;/ResendIfReplyNotHL7&gt;;cr;;lf;  &lt;ResendIfReplyNotHL7ACK&gt;True&lt;/ResendIfReplyNotHL7ACK&gt;;cr;;lf;  &lt;ResendOnReplyTimeout&gt;True&lt;/ResendOnReplyTimeout&gt;;cr;;lf;  &lt;SendHL7ACK&gt;True&lt;/SendHL7ACK&gt;;cr;;lf;  &lt;SendReply&gt;True&lt;/SendReply&gt;;cr;;lf;  &lt;UnlimitedReconnect&gt;True&lt;/UnlimitedReconnect&gt;;cr;;lf;  &lt;UnlimitedResend&gt;True&lt;/UnlimitedResend&gt;;cr;;lf;  &lt;RequestProcessingVerboseLogging&gt;False&lt;/RequestProcessingVerboseLogging&gt;;cr;;lf;  &lt;ResponseProcessingVerboseLogging&gt;False&lt;/ResponseProcessingVerboseLogging&gt;;cr;;lf;  &lt;LogPurge&gt;default&lt;/LogPurge&gt;;cr;;lf;  &lt;MSMQArrivalQueue&gt;.\private$\OB_UW_ORU_arrival&lt;/MSMQArrivalQueue&gt;;cr;;lf;  &lt;MSMQProcessQueue&gt;.\private$\OB_UW_ORU_process&lt;/MSMQProcessQueue&gt;;cr;;lf;  &lt;StaticReplyString&gt;&lt;/StaticReplyString&gt;;cr;;lf;  &lt;LogFlags&gt;21&lt;/LogFlags&gt;;cr;;lf;  &lt;LogStates&gt;0&lt;/LogStates&gt;;cr;;lf;  &lt;OnMSA1AA&gt;0&lt;/OnMSA1AA&gt;;cr;;lf;  &lt;OnMSA1AE&gt;1&lt;/OnMSA1AE&gt;;cr;;lf;  &lt;OnMSA1AR&gt;-1&lt;/OnMSA1AR&gt;;cr;;lf;  &lt;OnMSA1Other&gt;1&lt;/OnMSA1Other&gt;;cr;;lf;  &lt;ReconnectDelay&gt;5&lt;/ReconnectDelay&gt;;cr;;lf;  &lt;ReconnectMax&gt;5&lt;/ReconnectMax&gt;;cr;;lf;  &lt;ReplyTimeout&gt;30&lt;/ReplyTimeout&gt;;cr;;lf;  &lt;ResendMax&gt;5&lt;/ResendMax&gt;;cr;;lf;  &lt;SendDelay&gt;0&lt;/SendDelay&gt;;cr;;lf;  &lt;WaitIconTime&gt;10&lt;/WaitIconTime&gt;;cr;;lf;  &lt;AnnotationSets&gt;&amp;lt;Sets/&amp;gt;;cr;;lf;&lt;/AnnotationSets&gt;;cr;;lf;  &lt;IBConversionConfig&gt;;cr;;lf;    &lt;ConversionConfig version="1"&gt;;cr;;lf;      &lt;ConversionType&gt;0&lt;/ConversionType&gt;;cr;;lf;      &lt;DerivPath&gt;&lt;/DerivPath&gt;;cr;;lf;      &lt;EnforceRequiredFields&gt;True&lt;/EnforceRequiredFields&gt;;cr;;lf;      &lt;EnforceRequiredSegments&gt;True&lt;/EnforceRequiredSegments&gt;;cr;;lf;      &lt;HL7Version&gt;&lt;/HL7Version&gt;;cr;;lf;      &lt;IgnoreUnexpectedFields&gt;False&lt;/IgnoreUnexpectedFields&gt;;cr;;lf;      &lt;IgnoreUnexpectedSegments&gt;False&lt;/IgnoreUnexpectedSegments&gt;;cr;;lf;      &lt;PreQueueName&gt;.\private$\OB_UW_ORU_pre&lt;/PreQueueName&gt;;cr;;lf;      &lt;PostQueueName&gt;.\private$\OB_UW_ORU_post&lt;/PostQueueName&gt;;cr;;lf;      &lt;IsInbound&gt;True&lt;/IsInbound&gt;;cr;;lf;    &lt;/ConversionConfig&gt;;cr;;lf;  &lt;/IBConversionConfig&gt;;cr;;lf;  &lt;OBConversionConfig&gt;;cr;;lf;    &lt;ConversionConfig version="1"&gt;;cr;;lf;      &lt;ConversionType&gt;0&lt;/ConversionType&gt;;cr;;lf;      &lt;DerivPath&gt;&lt;/DerivPath&gt;;cr;;lf;      &lt;EnforceRequiredFields&gt;True&lt;/EnforceRequiredFields&gt;;cr;;lf;      &lt;EnforceRequiredSegments&gt;True&lt;/EnforceRequiredSegments&gt;;cr;;lf;      &lt;HL7Version&gt;&lt;/HL7Version&gt;;cr;;lf;      &lt;IgnoreUnexpectedFields&gt;False&lt;/IgnoreUnexpectedFields&gt;;cr;;lf;      &lt;IgnoreUnexpectedSegments&gt;False&lt;/IgnoreUnexpectedSegments&gt;;cr;;lf;      &lt;PreQueueName&gt;.\private$\OB_UW_ORU_pre&lt;/PreQueueName&gt;;cr;;lf;      &lt;PostQueueName&gt;.\private$\OB_UW_ORU_post&lt;/PostQueueName&gt;;cr;;lf;      &lt;IsInbound&gt;True&lt;/IsInbound&gt;;cr;;lf;    &lt;/ConversionConfig&gt;;cr;;lf;  &lt;/OBConversionConfig&gt;;cr;;lf;  &lt;IBTcpIpFraming&gt;;cr;;lf;    &lt;TcpIpFraming&gt;;cr;;lf;      &lt;FramingType&gt;0&lt;/FramingType&gt;;cr;;lf;      &lt;Header&gt;&lt;/Header&gt;;cr;;lf;      &lt;Trailer&gt;&lt;/Trailer&gt;;cr;;lf;    &lt;/TcpIpFraming&gt;;cr;;lf;  &lt;/IBTcpIpFraming&gt;;cr;;lf;  &lt;OBTcpIpFraming&gt;;cr;;lf;    &lt;TcpIpFraming&gt;;cr;;lf;      &lt;FramingType&gt;0&lt;/FramingType&gt;;cr;;lf;      &lt;Header&gt;&lt;/Header&gt;;cr;;lf;      &lt;Trailer&gt;&lt;/Trailer&gt;;cr;;lf;    &lt;/TcpIpFraming&gt;;cr;;lf;  &lt;/OBTcpIpFraming&gt;;cr;;lf;  &lt;TlsConfig&gt;;cr;;lf;    &lt;TlsConfig Version="1"&gt;;cr;;lf;      &lt;MutualAuthRequired&gt;False&lt;/MutualAuthRequired&gt;;cr;;lf;      &lt;PresentCertEncoded&gt;&lt;/PresentCertEncoded&gt;;cr;;lf;      &lt;PresentCertSubject&gt;&lt;/PresentCertSubject&gt;;cr;;lf;      &lt;AcceptCertEncoded&gt;&lt;/AcceptCertEncoded&gt;;cr;;lf;      &lt;AcceptAnyCert&gt;False&lt;/AcceptAnyCert&gt;;cr;;lf;      &lt;AcceptWrongCN&gt;False&lt;/AcceptWrongCN&gt;;cr;;lf;      &lt;AcceptExpired&gt;False&lt;/AcceptExpired&gt;;cr;;lf;    &lt;/TlsConfig&gt;;cr;;lf;  &lt;/TlsConfig&gt;;cr;;lf;  &lt;ConfigFacesParent&gt;;cr;;lf;    &lt;ConfigFace ConfigFace_ID="0"&gt;;cr;;lf;      &lt;TcpIpPort&gt;32500&lt;/TcpIpPort&gt;;cr;;lf;      &lt;SinkGearConfig&gt;;cr;;lf;        &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;      &lt;/SinkGearConfig&gt;;cr;;lf;      &lt;SourceGearConfig&gt;;cr;;lf;        &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;      &lt;/SourceGearConfig&gt;;cr;;lf;      &lt;EnableAlerts&gt;True&lt;/EnableAlerts&gt;;cr;;lf;      &lt;AutoStart&gt;True&lt;/AutoStart&gt;;cr;;lf;      &lt;TcpIpServer&gt;10.1.20.232&lt;/TcpIpServer&gt;;cr;;lf;      &lt;AlertConfigs&gt;;cr;;lf;        &lt;AlertConfigs&gt;;cr;;lf;          &lt;AlertConfig deriv="/Alerting" name="Alerting" /&gt;;cr;;lf;        &lt;/AlertConfigs&gt;;cr;;lf;      &lt;/AlertConfigs&gt;;cr;;lf;      &lt;SimulateConnect&gt;False&lt;/SimulateConnect&gt;;cr;;lf;      &lt;TlsEnabled&gt;False&lt;/TlsEnabled&gt;;cr;;lf;      &lt;MaxConcurrentConnections&gt;1&lt;/MaxConcurrentConnections&gt;;cr;;lf;      &lt;Schedules /&gt;;cr;;lf;    &lt;/ConfigFace&gt;;cr;;lf;    &lt;ConfigFace ConfigFace_ID="2"&gt;;cr;;lf;      &lt;TcpIpPort&gt;32501&lt;/TcpIpPort&gt;;cr;;lf;      &lt;SinkGearConfig&gt;;cr;;lf;        &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;      &lt;/SinkGearConfig&gt;;cr;;lf;      &lt;SourceGearConfig&gt;;cr;;lf;        &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;      &lt;/SourceGearConfig&gt;;cr;;lf;      &lt;EnableAlerts&gt;True&lt;/EnableAlerts&gt;;cr;;lf;      &lt;AutoStart&gt;True&lt;/AutoStart&gt;;cr;;lf;      &lt;TcpIpServer&gt;10.1.20.177&lt;/TcpIpServer&gt;;cr;;lf;      &lt;AlertConfigs&gt;;cr;;lf;        &lt;AlertConfigs&gt;;cr;;lf;          &lt;AlertConfig deriv="/Alerting" name="Alerting" /&gt;;cr;;lf;        &lt;/AlertConfigs&gt;;cr;;lf;      &lt;/AlertConfigs&gt;;cr;;lf;      &lt;SimulateConnect&gt;False&lt;/SimulateConnect&gt;;cr;;lf;      &lt;TlsEnabled&gt;False&lt;/TlsEnabled&gt;;cr;;lf;      &lt;MaxConcurrentConnections&gt;1&lt;/MaxConcurrentConnections&gt;;cr;;lf;      &lt;Schedules /&gt;;cr;;lf;    &lt;/ConfigFace&gt;;cr;;lf;  &lt;/ConfigFacesParent&gt;;cr;;lf;  &lt;ConnectionName&gt;OB_UW_ORU&lt;/ConnectionName&gt;;cr;;lf;  &lt;Generation&gt;0&lt;/Generation&gt;;cr;;lf;  &lt;DefaultLogPurge&gt;&lt;/DefaultLogPurge&gt;;cr;;lf;  &lt;CommCalloutProgId&gt;&lt;/CommCalloutProgId&gt;;cr;;lf;  &lt;TcpIpPort&gt;32500&lt;/TcpIpPort&gt;;cr;;lf;  &lt;SinkGearConfig&gt;;cr;;lf;    &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;  &lt;/SinkGearConfig&gt;;cr;;lf;  &lt;SourceGearConfig&gt;;cr;;lf;    &lt;GearConfig version="1" ProgId="" /&gt;;cr;;lf;  &lt;/SourceGearConfig&gt;;cr;;lf;  &lt;EnableAlerts&gt;True&lt;/EnableAlerts&gt;;cr;;lf;  &lt;AutoStart&gt;True&lt;/AutoStart&gt;;cr;;lf;  &lt;TcpIpServer&gt;10.1.20.232&lt;/TcpIpServer&gt;;cr;;lf;  &lt;AlertConfigs&gt;;cr;;lf;    &lt;AlertConfigs&gt;;cr;;lf;      &lt;AlertConfig deriv="/Alerting" name="Alerting" /&gt;;cr;;lf;    &lt;/AlertConfigs&gt;;cr;;lf;  &lt;/AlertConfigs&gt;;cr;;lf;  &lt;SimulateConnect&gt;False&lt;/SimulateConnect&gt;;cr;;lf;  &lt;TlsEnabled&gt;False&lt;/TlsEnabled&gt;;cr;;lf;  &lt;MaxConcurrentConnections&gt;1&lt;/MaxConcurrentConnections&gt;;cr;;lf;  &lt;Schedules /&gt;;cr;;lf;  &lt;AlertConfigDerivPath&gt;/Alerting&lt;/AlertConfigDerivPath&gt;;cr;;lf;  &lt;AlertConfigName&gt;Alerting&lt;/AlertConfigName&gt;;cr;;lf;&lt;/Connection&gt;</Object><Object Type="Threshold" Name="#sl#Alerting#sl#Queue#sp#Depth#sp#Handler" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="389">&lt;QueueDepthThreshold version="2" description="Generated by the Alert Wizard on 7/17/2013" requiresresolution="0"&gt;;cr;;lf;  &lt;Limits activation="30" deactivation="29" /&gt;;cr;;lf;  &lt;AlertLevel name="" /&gt;;cr;;lf;  &lt;Notification name="Alert Receipents" /&gt;;cr;;lf;  &lt;Actions&gt;;cr;;lf;    &lt;SendEmail notification="Alert Receipents" version="1"&gt;;cr;;lf;      &lt;Body replace="0" /&gt;;cr;;lf;    &lt;/SendEmail&gt;;cr;;lf;  &lt;/Actions&gt;;cr;;lf;&lt;/QueueDepthThreshold&gt;</Object><Object Type="Correlation" Name="#sl#Result#sp#Status" ModDateUtc="2013-06-06T15:52:45Z" ModDate="2013#sl#06#sl#06#sp#10:52:45" Length="811">&lt;Correlation Desc="" Embedded="0" CompatibleVersion="4.0.0" Version="1"&gt;&lt;Groups&gt;&lt;CodesetGroup Domain="1" Unique="1" Visible="1"&gt;&lt;Codesets&gt;&lt;Codeset Name="#sl#Extract#sp#Systems#sl#Extract#sp#Result#sp#Status" DescVisible="False" /&gt;&lt;/Codesets&gt;&lt;/CodesetGroup&gt;&lt;CodesetGroup Domain="0" Unique="1" Visible="1"&gt;&lt;Codesets&gt;&lt;Codeset Name="#sl#Extract#sp#Systems#sl#HIS#sp#Result#sp#Status" DescVisible="False" /&gt;&lt;/Codesets&gt;&lt;/CodesetGroup&gt;&lt;/Groups&gt;&lt;Primaries /&gt;&lt;Rows&gt;&lt;Array&gt;&lt;Element Value="Final" /&gt;&lt;Element Value="F" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="Preliminary" /&gt;&lt;Element Value="P" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="Reviewed" /&gt;&lt;Element Value="?" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;Array&gt;&lt;Element Value="Edited" /&gt;&lt;Element Value="C" /&gt;&lt;Element Value="" /&gt;&lt;/Array&gt;&lt;/Rows&gt;&lt;/Correlation&gt;</Object><Object Type="Threshold" Name="#sl#Alerting#sl#Stopped#sp#Handler" ModDateUtc="2013-07-17T23:48:29Z" ModDate="2013#sl#07#sl#17#sp#18:48:29" Length="358">&lt;TimeThreshold version="2" description="Generated by the Alert Wizard on 7/17/2013" requiresresolution="0"&gt;;cr;;lf;  &lt;Limits seconds="3600" /&gt;;cr;;lf;  &lt;AlertLevel name="" /&gt;;cr;;lf;  &lt;Notification name="Alert Receipents" /&gt;;cr;;lf;  &lt;Actions&gt;;cr;;lf;    &lt;SendEmail notification="Alert Receipents" version="1"&gt;;cr;;lf;      &lt;Body replace="0" /&gt;;cr;;lf;    &lt;/SendEmail&gt;;cr;;lf;  &lt;/Actions&gt;;cr;;lf;&lt;/TimeThreshold&gt;</Object><Object Type="Recipient" Name="#sl#Alerting#sl#UW#sp#Health#sp#Kris#sp#Shultz" ModDateUtc="2014-01-16T23:26:46Z" ModDate="2014#sl#01#sl#16#sp#17:26:46" Length="114">&lt;EmailRecipient description="" version="1"&gt;;cr;;lf;  &lt;EmailAddress address="KSchultz@uwhealth.org" /&gt;;cr;;lf;&lt;/EmailRecipient&gt;</Object><Object Type="Recipient" Name="#sl#Alerting#sl#UW#sp#Health#sp#Stefano#sp#Walczyk" ModDateUtc="2014-01-16T23:26:46Z" ModDate="2014#sl#01#sl#16#sp#17:26:46" Length="114">&lt;EmailRecipient description="" version="1"&gt;;cr;;lf;  &lt;EmailAddress address="SWalczyk@uwhealth.org" /&gt;;cr;;lf;&lt;/EmailRecipient&gt;</Object></Package>