Scroll down for information regarding RedactionXmlToXls.pl
####################################################################################
XmlToXls.pl
Purpose:
Parses all test result xml files in the current working directory to generate ouput suitable for copying 
and pasting into "Performance Results" spreadsheets. A file called "results.xls" is generated in the
current working directory.

Requires:
Perl 5.8.8 with the XML::Simple module installed.


Usage:
From a command prompt in the directory where the test result xml files are located:
perl XmlToXls.pl
Open results.xls, adjust attribute names as needed, and copy and paste the appropriate
data into your Performance Results spreadsheet.

Caveats:
Subattribute names are output as "ParentAttribute.Subattribute" and may need to be renamed.
The format may differ from the spreadsheets for certain counties. It currently works
only for tests which are configured in the tcl file to use UCLIDAFCoreTest.AutomatedRuleSetTester.1 .
Functionality for other kinds of tests may be added later.
####################################################################################
RedactionXmlToXls.pl
Purpose:
Parses all test result xml files in the current working directory to generate a
spreadsheet matching the current (June 2007) templates for recording IDShield
data, with the spreadsheet name derived from the working directory name.

Requires:
Perl 5.8.8 with the  Spreadsheet::WriteExcel modules installed.
An xml parser is not used.


Usage:
From a command prompt in the directory where the test result xml files are located:
RedactionXmlToXls.pl


Caveats:
This was tested on a small sample of test xml files. Please send feedback regarding any unexpected
behavior or output to mitch@extractsystems.com