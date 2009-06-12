#!/usr/bin/perl -w


use strict;
use Cwd;

		
sub ShowUsage()
{
	die "\nUsage:\nRandomSetFromList.pl <ListFileName> <Sample Size> [/t](Optional parameter to make Test Files)\n";
}
sub trim($)
{
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}
sub GetDirectoryFromString($)
{
	my $string = shift;
	if ($string =~ m/^K:.*\\/)
	{
		$string = trim($string);
		$string =~ s/(.+\\).+?$/$1/;
		return $string;
	}
	else
	{
		$string = cwd();
		$string .= "\/";
		return $string;
	}
}

sub GetEAVDirFromImgDir($)
{
	my $string = shift;
	if ($string =~ /^K:.+\\Images\\.+/)
	{
	$string =~ s/(.+\\)Images\\.+/$1/;
	$string .= "EAV\\TestAttribute";
	return $string;
	}
	else
	{
		return "EAV_File_Dir";
	}
	
}
sub GetFileNameFromPath($)
{
	my $string = shift;
	$string = trim($string);
	$string =~ s/.+\\//;
	return $string;
}
my $SampleSize;
my $FileList;
my @Selected;
my @AllFiles;
my @RandomSample;
my $OutputPath;
my $SetDir = "";


ShowUsage() if ((!((defined($ARGV[0]) && defined($ARGV[1]))))||(defined($ARGV[3])));
ShowUsage() if ((defined($ARGV[2])) && (!($ARGV[2] eq "/t")));
open (LIST, "$ARGV[0]") || die "Can't open $ARGV[0]\nUsage:\nRandomSetFromList.pl <ListFileName> <Sample Size> [/t](Optional parameter to make Test Files)\n";
$OutputPath = $ARGV[0];
$OutputPath =~ s/\.\w+$//;

$SampleSize = int($ARGV[1]) ||die "Not Valid Number.\nUsage:\nRandomSetFromList.pl [filename or current dir] <Sample Size> [<File Extension>]\n"; 
die "\nNot an Integer.\nUsage:\nRandomSetFromList.pl <ListFileName> <Sample Size> [/t](Optional parameter to make Test Files)\n"  unless ($ARGV[1] == $SampleSize );

while(<LIST>)
{
	if (($SetDir eq "") && ($_ =~ m/.*\w.*/))
	{
	$SetDir = GetDirectoryFromString($_);
#	print "Set Directory: $SetDir\n";
	}
	push(@AllFiles, trim($_)) if ($_ =~ m/.*\w.*/);
}

close(LIST);
die "Sample size larger than file list\n" unless ($SampleSize < scalar(@AllFiles));
	for (my $i = 0;$i< scalar(@AllFiles);$i++)
	{
		$Selected[$i] = 0;
	}	
	until (scalar(@RandomSample) == $SampleSize)

	{
		my $RandomIndex = int(rand(scalar(@AllFiles)));
		if ($Selected[$RandomIndex] == 0)
		{ 
			push (@RandomSample,  $AllFiles[$RandomIndex]);
			$Selected[$RandomIndex] = 1;
			
		}
		
	}
	#Sort the list of paths in "string" order.
	@RandomSample = sort {$a cmp $b} @RandomSample;
# Tested earlier for if ($ARGV[2]" eq /t")
if (defined($ARGV[2]))
{
	$OutputPath = "Random" . "$SampleSize" . "_" . $OutputPath;
	my $TestFileDirV3 = "TestingFiles_" . $OutputPath . "_V3";
	my $TestFileDirV5 = "TestingFiles_" . $OutputPath . "_V5";
	
	unless (-d $TestFileDirV3)
	{
		mkdir ($TestFileDirV3, 0755) || die "Cannot create $TestFileDirV3.\n";
	}
	unless (-d $TestFileDirV5)
	{
		mkdir ($TestFileDirV5, 0755) || die "Cannot create $TestFileDirV5.\n";
	}
	my $TestFilePathV3 = $TestFileDirV3 . "\\" . $OutputPath . "\.dat";
	my $TestFilePathV5 = $TestFileDirV5 . "\\" . $OutputPath . "\.dat";
	open (V3TESTFILE, ">$TestFilePathV3");
	open (V5TESTFILE, ">$TestFilePathV5");
	my $TCLFilePathV3 = $TestFilePathV3;
	my $TCLFilePathV5 = $TestFilePathV5;
	$TCLFilePathV3 =~ s/\.dat/\.tcl/;
	$TCLFilePathV5 =~ s/\.dat/\.tcl/;
	open (V3TCLFILE, ">$TCLFilePathV3");
	open (V5TCLFILE, ">$TCLFilePathV5");
	print V3TCLFILE "UCLIDAFCoreTest.AutomatedRuleSetTester.1;;" . GetFileNameFromPath($TestFilePathV3) . "\n";
	print V5TCLFILE "UCLIDAFCoreTest.AutomatedRuleSetTester.1;;" . GetFileNameFromPath($TestFilePathV5) . "\n";
	close (V3TCLFILE);
	close (V5TCLFILE);
	print V3TESTFILE "<SETTING>;EAV_MUST_EXIST=TRUE\n\/\/Test File Template- Change \"TestAttribute\" to an appropriate value.\n";
	print V5TESTFILE "<SETTING>;EAV_MUST_EXIST=TRUE\n\/\/Test File Template- Change \"TestAttribute\" to an appropriate value.\n";
	for my $Element (@RandomSample)
	{
		my $EavDir = GetEAVDirFromImgDir($Element);
		my $EavFile = $EavDir ."\\" . GetFileNameFromPath($Element). ".eav";
		print V3TESTFILE qq(<TESTCASE>;TestAttribute.rsd;$Element.uss;$EavFile\n);
		print V5TESTFILE qq(<TESTCASE>;TestAttribute.rsd;$Element.uss;;$EavFile\n);
	}
	close (V3TESTFILE);
	close (V5TESTFILE);
	print "\nTest Files:\n$TestFilePathV3 and\n$TestFileDirV5\n";
}
else
{
$OutputPath = "MakeRandom" . "$SampleSize" . "_" . $OutputPath . ".bat";
open (OUTFILE, ">$OutputPath");
$OutputPath =~ s/Make(.+)\.bat$/$1/;
print OUTFILE qq(mkdir "$OutputPath"\n);
for my $Element (@RandomSample)
{
	print OUTFILE qq(copy "$Element" "$OutputPath\\"\n);
	print OUTFILE qq(copy "$Element.uss" "$OutputPath\\"\n);
}
print "\nBatch file to make random set:\nMake$OutputPath.bat\n";
}
close (OUTFILE);
