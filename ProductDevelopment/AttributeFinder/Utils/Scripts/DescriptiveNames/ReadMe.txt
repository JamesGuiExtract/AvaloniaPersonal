Purpose:
To copy test result xml files with the default names given by the Test
Harness application to files with names showing what type of documents were
tested, useful mainly for GrantorGrantee and Document Classification tests
where a directory with multiple test result xml files is generated.
For example, invoking the script as
perl DescriptiveName.pl Fresno_ _FKBVer22
will copy files named:
UCLIDAFUtilsTest.DocumentClassifierTester.1 - 11-22-2006 - 06.35.26.xml
UCLIDAFUtilsTest.DocumentClassifierTester.1 - 11-22-2006 - 06.44.55.xml
UCLIDAFUtilsTest.DocumentClassifierTester.1 - 11-22-2006 - 06.51.47.xml
UCLIDAFUtilsTest.DocumentClassifierTester.1 - 11-22-2006 - 06.22.39.xml
to files named:
Fresno_AbstractOfSupportJudgment_FKBVer22.xml
Fresno_AcknowledgementOfSatisfactionofJudgment_FKBVer22.xml
Fresno_AffidavitOfDeath_FKBVer22.xml
Fresno_AbstractOfJudgment_FKBVer22.xml 

Usage:
perl DescriptiveName.pl Argument1 [ Argument2 ]
Copies each file found in the directory from which it is invoked
to a file with a name constructed from:
the name of the directory containing the tested uss files prepended with
Argument1 and appended with Argument2 when invoked with 2 arguments
or
the name of the directory containing the tested uss files appended with
Argument1 when invoked with 1 arguments.


Requirements:
Perl 5
XML::Simple module, available from CPAN

Caveats:
This script is really useful only in situations where the documents tested
are in directories whose names correspond with the document types.

If it is run in the same directory more than once, copies of the files
created in the first run of the script will be made, so care needs
to be taken to run it only once.

