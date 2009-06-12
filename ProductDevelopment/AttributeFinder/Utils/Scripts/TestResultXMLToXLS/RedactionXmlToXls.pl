#!/usr/bin/perl

# use module
#use File::Listing;
use XML::Simple;
use Cwd;
#diagnostic:
#use Data::Dumper;
use Spreadsheet::WriteExcel;
## Some utility Functions ##
sub trim($)
{
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}
sub GetDateFromFileName($)
{
	my $DateString = shift;
	if($DateString =~ m/\b(?:\d{2}\-){2}\d{4}\b/)
	{
		$DateString =~ s/.+(\b(?:\d{2}\-){2}\d{4}\b).+/$1/;
		$DateString =~ s/\-/\//g;
	}
	else
	{
		$DateString = "Type date here.";	
	}
	return $DateString;
}
## Declare Variables      ##
my $CurrentWorkingDirectory = cwd();
$CurrentWorkingDirectory =~ s/.+\///;
my $OutputFile = $CurrentWorkingDirectory . "_results.xls";
my $PassedExcelFriendly = 0;
my $FoundDatFile = 0;
my $PassedQueryForAutomation = 0;
my $TotalFiles;
my $ElapsedTime;
my $FilesWithExpectedRedactions;
my $TotalExpectedRedactions;
my $ExpectedRedactions;
my $NumRedactionsFound;
my $NumFalsePositives;
my $FilesSelectedForReview;
my $NumExpectedRedactionsInReviewedFiles;
my $QueryForAutomation;
my $QueryForVerification;
my $TestDate;
my $RowCounter = 2;

#Open and read directory
opendir (RESDIR, ".");
my  @dirlist = grep  /.+\.xml$/, readdir(RESDIR);
closedir (RESDIR);
# Setup Spreadsheet
my $workbook = Spreadsheet::WriteExcel->new("$OutputFile") || die("Cannot open $OutputFile for writing\n");
my $sheet1 = $workbook->add_worksheet("Sheet1");
$workbook->add_worksheet("Sheet2");
$workbook->add_worksheet("Sheet3");

my $frmlRedactionCaptureRate = $sheet1->store_formula('=IF(AND(AND(E3<>"", E3<>0),G3<>""),G3/E3,"")'); 
my $frmlROCE = $sheet1->store_formula('=IF(AND(AND(I3<>"", I3<>0),G3<>""),G3/I3,"")');
my $frmlFilesToVerifyPercentage = $sheet1->store_formula('=IF(AND(AND(C3<>"", C3<>0),L3<>""),L3/C3,"")');
my $frmlPercentRedactionsInVerifiedFiles = $sheet1->store_formula('=IF(AND(AND(E3<>"", E3<>0),N3<>""),N3/E3,"")');


my $fmtBlueBGVerticalBold = $workbook->add_format();
$fmtBlueBGVerticalBold->set_bg_color(31);
$fmtBlueBGVerticalBold->set_bold();
$fmtBlueBGVerticalBold->set_border();
$fmtBlueBGVerticalBold->set_rotation(90);
$fmtBlueBGVerticalBold->set_text_wrap();
$fmtBlueBGVerticalBold->set_align('left');
$fmtBlueBGVerticalBold->set_locked();

my $fmtBlueBGVerticalBoldHvyRight = $workbook->add_format();
$fmtBlueBGVerticalBoldHvyRight->set_bg_color(31);
$fmtBlueBGVerticalBoldHvyRight->set_bold();
$fmtBlueBGVerticalBoldHvyRight->set_border();
$fmtBlueBGVerticalBoldHvyRight->set_rotation(90);
$fmtBlueBGVerticalBoldHvyRight->set_text_wrap();
$fmtBlueBGVerticalBoldHvyRight->set_align('left');
$fmtBlueBGVerticalBoldHvyRight->set_locked();
$fmtBlueBGVerticalBoldHvyRight->set_right(2);

my $fmtBlueBGVerticalBoldHvyLeft = $workbook->add_format();
$fmtBlueBGVerticalBoldHvyLeft->set_bg_color(31);
$fmtBlueBGVerticalBoldHvyLeft->set_bold();
$fmtBlueBGVerticalBoldHvyLeft->set_border();
$fmtBlueBGVerticalBoldHvyLeft->set_rotation(90);
$fmtBlueBGVerticalBoldHvyLeft->set_text_wrap();
$fmtBlueBGVerticalBoldHvyLeft->set_align('left');
$fmtBlueBGVerticalBoldHvyLeft->set_locked();
$fmtBlueBGVerticalBoldHvyLeft->set_left(2);

#merged version of above to accomodate limitation of Spreadsheet::WriteExcel
$fmtMergedBlueBGVerticalBold = $workbook->add_format();
$fmtMergedBlueBGVerticalBold->set_bg_color(31);
$fmtMergedBlueBGVerticalBold->set_bold();
$fmtMergedBlueBGVerticalBold->set_border();
$fmtMergedBlueBGVerticalBold->set_rotation(90);
$fmtMergedBlueBGVerticalBold->set_text_wrap();
$fmtMergedBlueBGVerticalBold->set_align('left');
$fmtMergedBlueBGVerticalBold->set_locked();

$fmtMergedBlueBGVerticalBoldHvyRight = $workbook->add_format();
$fmtMergedBlueBGVerticalBoldHvyRight->set_bg_color(31);
$fmtMergedBlueBGVerticalBoldHvyRight->set_bold();
$fmtMergedBlueBGVerticalBoldHvyRight->set_border();
$fmtMergedBlueBGVerticalBoldHvyRight->set_rotation(90);
$fmtMergedBlueBGVerticalBoldHvyRight->set_text_wrap();
$fmtMergedBlueBGVerticalBoldHvyRight->set_right(2);
$fmtMergedBlueBGVerticalBoldHvyRight->set_align('left');
$fmtMergedBlueBGVerticalBoldHvyRight->set_locked();

$fmtMergedBlueBGVerticalBoldHvyLeft = $workbook->add_format();
$fmtMergedBlueBGVerticalBoldHvyLeft->set_bg_color(31);
$fmtMergedBlueBGVerticalBoldHvyLeft->set_bold();
$fmtMergedBlueBGVerticalBoldHvyLeft->set_border();
$fmtMergedBlueBGVerticalBoldHvyLeft->set_rotation(90);
$fmtMergedBlueBGVerticalBoldHvyLeft->set_text_wrap();
$fmtMergedBlueBGVerticalBoldHvyLeft->set_left(2);
$fmtMergedBlueBGVerticalBoldHvyLeft->set_align('left');
$fmtMergedBlueBGVerticalBoldHvyLeft->set_locked();


my $fmtBlueBGBold = $workbook->add_format();
$fmtBlueBGBold->set_bg_color(31);
$fmtBlueBGBold->set_bold();
$fmtBlueBGBold->set_border();
$fmtBlueBGBold->set_text_wrap();
$fmtBlueBGBold->set_locked();

my $fmtMergedBlueBGBold = $workbook->add_format();
$fmtMergedBlueBGBold->set_bg_color(31);
$fmtMergedBlueBGBold->set_bold();
$fmtMergedBlueBGBold->set_border();
$fmtMergedBlueBGBold->set_text_wrap();
$fmtMergedBlueBGBold->set_locked();

my $fmtYellowBGBold = $workbook->add_format();
$fmtYellowBGBold->set_bg_color(43);
$fmtYellowBGBold->set_bold();
$fmtYellowBGBold->set_align('center');
$fmtYellowBGBold->set_border();

my $fmtYellowBGBoldPercent = $workbook->add_format();
$fmtYellowBGBoldPercent->set_bg_color(43);
$fmtYellowBGBoldPercent->set_bold();
$fmtYellowBGBoldPercent->set_align('center');
$fmtYellowBGBoldPercent->set_border();
$fmtYellowBGBoldPercent->set_num_format('0.0%');

my $fmtYellowBGBoldTwoPlace = $workbook->add_format();
$fmtYellowBGBoldTwoPlace->set_bg_color(43);
$fmtYellowBGBoldTwoPlace->set_bold();
$fmtYellowBGBoldTwoPlace->set_align('center');
$fmtYellowBGBoldTwoPlace->set_border();
$fmtYellowBGBoldTwoPlace->set_num_format('0.00');

my $fmtMergedGreyBGBold = $workbook->add_format();
$fmtMergedGreyBGBold->set_bg_color(22);
$fmtMergedGreyBGBold->set_bold();
$fmtMergedGreyBGBold->set_align('center');
$fmtMergedGreyBGBold->set_border();
$fmtMergedGreyBGBold->set_locked();
$fmtMergedGreyBGBold->set_right(2);
$fmtMergedGreyBGBold->set_left(2);

my $fmtWhiteCentered = $workbook->add_format();
$fmtWhiteCentered->set_border(); 
$fmtWhiteCentered->set_align('center');

my $fmtWhiteCenteredHvyLeft = $workbook->add_format();
$fmtWhiteCenteredHvyLeft->set_border(); 
$fmtWhiteCenteredHvyLeft->set_left(2); 
$fmtWhiteCenteredHvyLeft->set_align('center');

#Write column headings
$sheet1->write_blank(0,0);
$sheet1->write_blank(0,1);
$sheet1->write_blank(0,2);
$sheet1->write_blank(0,3);
$sheet1->write_blank(0,4);
$sheet1->merge_range(0,5,0,9,"Automated",$fmtMergedGreyBGBold);
$sheet1->merge_range(0,10,0,14,"Verified",$fmtMergedGreyBGBold);
$sheet1->write_blank(0,14);
$sheet1->write_blank(0,15);
$sheet1->write_blank(0,16);
$sheet1->write_blank(0,17);
$sheet1->write_blank(0,18);

$sheet1->merge_range(0,0,1,0,"Date",$fmtMergedBlueBGBold);
$sheet1->merge_range(0,1,1,1,"Testing Set",$fmtMergedBlueBGBold);
$sheet1->merge_range(0,2,1,2,"Files",$fmtMergedBlueBGVerticalBold);
$sheet1->merge_range(0,3,1,3,"Files w/ expected redactions",$fmtMergedBlueBGVerticalBold);
$sheet1->merge_range(0,4,1,4,"Expected redactions",$fmtMergedBlueBGVerticalBold);
$sheet1->write(1,5,"Query for Automation",$fmtBlueBGVerticalBoldHvyLeft);
$sheet1->write(1,6,"Redactions found",$fmtBlueBGVerticalBold);
$sheet1->write(1,7,"% redactions found",$fmtBlueBGVerticalBold);
$sheet1->write(1,8,"False Positives",$fmtBlueBGVerticalBold);
$sheet1->write(1,9,"ROCE",$fmtBlueBGVerticalBoldHvyRight);
$sheet1->write(1,10,"Query for Verification",$fmtBlueBGVerticalBold);
$sheet1->write(1,11,"Files to verify",$fmtBlueBGVerticalBold);
$sheet1->write(1,12,"% Files to verify",$fmtBlueBGVerticalBold);
$sheet1->write(1,13,"Redactions found",$fmtBlueBGVerticalBold);
$sheet1->write(1,14,"% redactions in reviewed files",$fmtBlueBGVerticalBoldHvyRight);
$sheet1->merge_range(0,15,1,15,"Time Elapsed (Secs)",$fmtMergedBlueBGVerticalBold);
$sheet1->merge_range(0,16,1,16,"Tester",$fmtMergedBlueBGVerticalBold);
$sheet1->merge_range(0,17,1,17,"Test Machine",$fmtMergedBlueBGVerticalBold);
$sheet1->merge_range(0,18,1,18,"Build # / FKB update",$fmtMergedBlueBGBold);
$sheet1->merge_range(0,19,1,19,"Notes",$fmtMergedBlueBGBold);
#end set up spreadsheet
# Process each xml file
foreach $F (@dirlist)
{
	open(TESTXML, $F) || die ("Can't open test xml file");
	while(<TESTXML>)
	{
		SWITCH:{
			if ($FoundDatFile == 0)
			{
				if ($_ =~ m/.*\.dat$/)
					{
						$DatFileName = trim($_);
						$DatFileName =~ s/^.+(?:\\|\/)//;
						$DatFileName =~ s/^.+?((?:Random|Subset|Set).+$)/$1/;
						$DatFileName =~ s/\.dat\b//;
						print "\nNow processing: $F\nBased on: $DatFileName\n";
						$FoundDatFile++;
						last SWITCH;
					}
			}
			if ($_ =~ m/Total\sfiles\sproc/)
				{
					$TotalFiles = trim($_);
					$TotalFiles =~ s/\D//g;
					print "Total Files:" . $TotalFiles ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Total\sExpected\sRed/)
				{
					$TotalExpectedRedactions = trim($_);
					$TotalExpectedRedactions =~ s/\D//g;
					print "Total Expected Redactions:" . $TotalExpectedRedactions ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Number\sof\sfiles.+red/)
				{
					$FilesWithExpectedRedactions = trim($_);
					$FilesWithExpectedRedactions =~ s/\D+(\d+)\D+.*/$1/;
					print "Files with Expected Redactions:" . trim($FilesWithExpectedRedactions) . "\n";
					last SWITCH;
				}
			if ($_ =~ m/Number\sof\sred.+/)
				{
					$NumRedactionsFound = trim($_);
					$NumRedactionsFound =~ s/\D+(\d+)\D+.*/$1/;
					print "Number of Redactions found:" . $NumRedactionsFound ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Number\sof\sfals.+/)
				{
					$NumFalsePositives = trim($_);
					$NumFalsePositives =~ s/\D+(\d+)\D+.*/$1/;
					print "Number of False Positives found:" . $NumFalsePositives ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Number\sof\sfi.+rev/)
				{
					$FilesSelectedForReview = trim($_);
					$FilesSelectedForReview =~ s/\D+(\d+)\D+.*/$1/;
					print "Number of Files Selected for Review:" . $FilesSelectedForReview ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Number\sof\sexp.+rev/)
				{
					$NumExpectedRedactionsInReviewedFiles = trim($_);
					$NumExpectedRedactionsInReviewedFiles =~ s/\D+(\d+)\D+.*/$1/;
					print "Number of Expected Redactions found in Reviewed Files:" . $NumExpectedRedactionsInReviewedFiles ."\n";
					last SWITCH;
				}
			if ($_ =~ m/Excel\spaste/)
				{
					$PassedExcelFriendly++;
				}
			if (($_ =~ m/^(?:HC|MC|LC)Data\|(?:HC|MC|LC)Data/) && ($PassedExcelFriendly > 0))
				{
					if ($PassedQueryForAutomation == 0)
					{
					$QueryForAutomation = trim($_);
					$QueryForAutomation =~ s/Data//g;
					print "Query for Automation:" . $QueryForAutomation ."\n";
					$PassedQueryForAutomation++;
					}
					else
					{
					$QueryForVerification = trim($_);
					$QueryForVerification =~ s/Data//g;
					print "Query for Verification:" . $QueryForVerification ."\n";
					}
					last SWITCH;
				}
			if ($_ =~ m/Total\selap.+/)
				{
					$ElapsedTime = trim($_);
					$ElapsedTime =~ s/\D+(\d+)\D+.*/$1/;
					print "Elapsed Time:" . $ElapsedTime ."\n";
					last SWITCH;
				}
			}
	}
	
	
	close(TESTXML);
	# Set FoundDatFile back to 0
	$FoundDatFile = 0;

	my $TestDate = GetDateFromFileName($F); 
	$sheet1->write($RowCounter,0,$TestDate,$fmtWhiteCentered);
	$sheet1->write($RowCounter,1,$DatFileName,$fmtWhiteCentered);
	$sheet1->write($RowCounter,2,$TotalFiles,$fmtWhiteCentered);
	$sheet1->write($RowCounter,3,$FilesWithExpectedRedactions,$fmtWhiteCentered);
	$sheet1->write($RowCounter,4,$TotalExpectedRedactions,$fmtWhiteCentered);
	$sheet1->write($RowCounter,5,$QueryForAutomation,$fmtWhiteCenteredHvyLeft);
	$sheet1->write($RowCounter,6,$NumRedactionsFound,$fmtWhiteCentered);
	$sheet1->repeat_formula($RowCounter,7,$frmlRedactionCaptureRate,$fmtYellowBGBoldPercent,qw/^E3$/,'E' . ($RowCounter + 1),qw/^E3$/,'E' . ($RowCounter + 1),qw/^G3$/,'G' . ($RowCounter + 1),qw/^G3$/,'G' . ($RowCounter + 1),qw/^E3$/,'E' . ($RowCounter + 1));
	$sheet1->write($RowCounter,8,$NumFalsePositives,$fmtWhiteCentered);
	$sheet1->repeat_formula($RowCounter,9,$frmlROCE,$fmtYellowBGBoldTwoPlace,qw/^I3$/,'I' . ($RowCounter + 1),qw/^I3$/,'I' . ($RowCounter + 1),qw/^G3$/,'G' . ($RowCounter + 1),qw/^G3$/,'G' . ($RowCounter + 1),qw/^I3$/,'I' . ($RowCounter + 1));
	$sheet1->write($RowCounter,10,$QueryForVerification,$fmtWhiteCenteredHvyLeft);
	$sheet1->write($RowCounter,11,$FilesSelectedForReview,$fmtWhiteCentered);
	$sheet1->repeat_formula($RowCounter,12,$frmlFilesToVerifyPercentage,$fmtYellowBGBoldPercent,qw/^C3$/,'C' . ($RowCounter + 1),qw/^C3$/,'C' . ($RowCounter + 1),qw/^L3$/,'L' . ($RowCounter + 1),qw/^L3$/,'L' . ($RowCounter + 1),qw/^C3$/,'C' . ($RowCounter + 1));
	$sheet1->write($RowCounter,13,$NumExpectedRedactionsInReviewedFiles,$fmtWhiteCentered);
	$sheet1->repeat_formula($RowCounter,14,$frmlPercentRedactionsInVerifiedFiles,$fmtYellowBGBoldPercent,qw/^E3$/,'E' . ($RowCounter + 1),qw/^E3$/,'E' . ($RowCounter + 1),qw/^N3$/,'N' . ($RowCounter + 1),qw/^N3$/,'N' . ($RowCounter + 1),qw/^E3$/,'E' . ($RowCounter + 1));
	$sheet1->write($RowCounter,15,$ElapsedTime,$fmtWhiteCenteredHvyLeft);
	$sheet1->write_blank($RowCounter,16,$fmtWhiteCentered);
	$sheet1->write_blank($RowCounter,17,$fmtWhiteCentered);
	$sheet1->write_blank($RowCounter,18,$fmtWhiteCentered);
	$sheet1->write_blank($RowCounter,19,$fmtWhiteCentered);
	$RowCounter++;
	
}
## Now add blank cells
while ($RowCounter < 10)
{
$sheet1->write_blank($RowCounter,0,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,1,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,2,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,3,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,4,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,5,$fmtWhiteCenteredHvyLeft);
$sheet1->write_blank($RowCounter,6,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,7,$fmtYellowBGBoldPercent);
$sheet1->write_blank($RowCounter,8,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,9,$fmtYellowBGBoldTwoPlace);
$sheet1->write_blank($RowCounter,10,$fmtWhiteCenteredHvyLeft);
$sheet1->write_blank($RowCounter,11,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,12,$fmtYellowBGBoldPercent);
$sheet1->write_blank($RowCounter,13,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,14,$fmtYellowBGBoldTwoPlace);
$sheet1->write_blank($RowCounter,15,$fmtWhiteCenteredHvyLeft);
$sheet1->write_blank($RowCounter,16,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,17,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,18,$fmtWhiteCentered);
$sheet1->write_blank($RowCounter,19,$fmtWhiteCentered);
$RowCounter++;
}
my $DatFileWidth = length($DatFileName);
$sheet1->set_column(1,1,$DatFileWidth);
my $QueryForVerificationWidth = length($QueryForVerification);
$sheet1->set_column(10,10,$QueryForVerificationWidth);
my $QueryForAutomationWidth = length($QueryForAutomation);
$sheet1->set_column(5,5,$QueryForAutomationWidth);
#Notes
$sheet1->set_column(19,19,32);
$sheet1->set_column(16,16,4);
#Files Column
$sheet1->set_column(0,0,10);
$sheet1->set_column(2,2,6);
$sheet1->set_column(8,8,6);
$sheet1->set_column(9,9,6);
$sheet1->set_column(11,11,6);
$sheet1->set_column(13,13,6);
$sheet1->set_column(15,15,6);

print "\nResults will be found in: $OutputFile\n";
