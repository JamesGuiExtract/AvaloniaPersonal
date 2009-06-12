#!/usr/bin/perl -w


use strict;
#Do we need current working directory?
#use Cwd;
use Spreadsheet::WriteExcel;
my $SampleSize;
# Default to tif.
my $ImageFileExt = "tif";
my @DirList;
#my @Counts;
#my @ImageLists;
my %DirsCounts;
my %DirsPaths;
my $DirColumnWidth = 24;
my $CumulativePercent = 0;
my $Passed80 = 0;
my $intRemainingStart;

die "\nUsage:\nFileDistToXls.pl FileExtension,\nallowing one or two extensions.\n" if ((defined($ARGV[0])) && (!($ARGV[0] =~ m/^\w{1,4}(?:\.\w{1,4})?$/)) || (defined($ARGV[1]))||!(defined($ARGV[0])));
$ImageFileExt = $ARGV[0]; 
#from http://www.perl.com/doc/FAQs/FAQ/oldfaq-html/Q4.13.html
sub round {
    my($number) = shift;
    return int($number + .5);
}

sub trim($)
{
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}
for my $eachDir (glob('./*')) 
{

	if( -d $eachDir)
	{
#		print "\t",$eachDir,"\n";
		push (@DirList, $eachDir);
	} 

}

foreach my $FDir (@DirList)
{
my @TempImageArray;
opendir(DIR, "$FDir");
@TempImageArray = grep(/\.$ImageFileExt$/,readdir(DIR));
closedir(DIR);
$DirsCounts{$FDir}=scalar(@TempImageArray);
$DirsPaths{$FDir}=\@TempImageArray;
}







my $Total = 0;

foreach my $key (keys (%DirsCounts))
{
#	print "$DirsCounts{$key}\n";
	$Total += $DirsCounts{$key};
}
die "No .$ImageFileExt files found.\n" if ($Total == 0);
print "Total $ImageFileExt Files: $Total \n";

# sort DirsCounts by Value, then reverse sort  by key
my @DirsByCount = reverse(sort { $DirsCounts{$a} <=> $DirsCounts{$b} || $b cmp $a} keys %DirsCounts);
my $RunningTotal = 0;
my $Once = 0;
#create Excel File
my $workbook = Spreadsheet::WriteExcel->new("FileDistribution.xls");
my $sheet1 = $workbook->add_worksheet("File Distribution");
my $fmtBlueBGBold = $workbook->add_format();
$fmtBlueBGBold->set_bg_color(31);
$fmtBlueBGBold->set_bold();
$fmtBlueBGBold->set_border();
$fmtBlueBGBold->set_text_wrap();
$fmtBlueBGBold->set_locked();

my $fmtBlueBGBoldMerged = $workbook->add_format();
$fmtBlueBGBoldMerged->set_bg_color(31);
$fmtBlueBGBoldMerged->set_bold();
$fmtBlueBGBoldMerged->set_border();
$fmtBlueBGBoldMerged->set_text_wrap();
$fmtBlueBGBoldMerged->set_locked();
#my $fmtYellowBGBold = $workbook->add_format();
#$fmtYellowBGBold->set_bg_color(43);
#$fmtYellowBGBold->set_bold();
#$fmtYellowBGBold->set_align('center');
#$fmtYellowBGBold->set_border();

my $fmtLtYellowBGBold = $workbook->add_format();
$fmtLtYellowBGBold->set_bg_color(26);
$fmtLtYellowBGBold->set_bold();
$fmtLtYellowBGBold->set_align('left');
$fmtLtYellowBGBold->set_border();

my $fmtLtYellowBG = $workbook->add_format();
$fmtLtYellowBG->set_bg_color(26);
$fmtLtYellowBG->set_align('left');
$fmtLtYellowBG->set_border();

my $fmtGrey = $workbook->add_format();
$fmtGrey->set_bg_color(22);
$fmtGrey->set_border(); 
$fmtGrey->set_bold(); 
$fmtGrey->set_align('left');
$fmtGrey->set_align('top');

my $fmtWhite = $workbook->add_format();
$fmtWhite->set_border(); 
$fmtWhite->set_num_format('0');
$fmtWhite->set_align('left');

my $fmtPeach = $workbook->add_format();
$fmtPeach->set_border(); 
$fmtPeach->set_bold(); 
$fmtPeach->set_bg_color(47);
$fmtPeach->set_align('left');

my $fmtWhiteInt = $workbook->add_format();
$fmtWhiteInt->set_border(); 
$fmtWhiteInt->set_bold(); 
$fmtWhiteInt->set_num_format('0');
$fmtWhiteInt->set_align('left');

my $fmtWhitePercent = $workbook->add_format();
$fmtWhitePercent->set_border(); 
$fmtWhitePercent->set_bold(); 
$fmtWhitePercent->set_num_format('0.0%');
$fmtWhitePercent->set_align('left');

my $fmtLtYellowBGBoldPercent = $workbook->add_format();
$fmtLtYellowBGBoldPercent->set_bg_color(26);
$fmtLtYellowBGBoldPercent->set_bold();
$fmtLtYellowBGBoldPercent->set_align('left');
$fmtLtYellowBGBoldPercent->set_border();
$fmtLtYellowBGBoldPercent->set_num_format('0.0%');

$sheet1->write(0,0,"Image Directory Name",$fmtBlueBGBold);
$sheet1->write(0,1,"Count",$fmtBlueBGBold);
$sheet1->write(0,2,"Percent",$fmtBlueBGBold);
$sheet1->write(0,3,"Cumulative\nPercent",$fmtBlueBGBold);
$sheet1->merge_range(0,4,0,5,"Top 80 Percent /\nBottom 20 Percent",$fmtBlueBGBoldMerged);
#print "FileDir,Count,Percentage\n";
my $RowCounter = 1;
my $LastCell = (scalar(@DirsByCount) + 1);

my $frmlDirPercent = $sheet1->store_formula('=B2/SUM($B$2:$B$99)');
my $frmlCumulativePercent = $sheet1->store_formula('=SUM($B$2:B2)/SUM($B$2:$B$99)');
#my $frmlCumulativePercent = $sheet1->store_formula('=Sum($B$2:B3)/Sum($B$2:$B$'. $LastCell . ')');
#my $frmlCumulativePercent = $sheet1->store_formula('=Sum($B$2:B3)/Sum($B$2:$B$'. $LastCell . ')');
foreach my $Cnt (@DirsByCount)
{ 
	my $Percentage = $DirsCounts{$Cnt}/$Total;
	my $Dir = $Cnt;
	$Dir =~ s/^\W+//;
#	print qq("$Dir","$DirsCounts{$Cnt}","$Percentage"\n);
	$CumulativePercent += $Percentage;
#	print "Cumultative\nPercent: $CumulativePercent\n";
	$DirColumnWidth = length($Dir) if (length($Dir) > $DirColumnWidth);
	if (($CumulativePercent <= .80) || ($Passed80 < 1))
	{
	$sheet1->write($RowCounter,0,$Dir,$fmtLtYellowBG);
	$sheet1->repeat_formula($RowCounter,3,$frmlCumulativePercent,$fmtLtYellowBGBoldPercent ,qw/B2$/,'B' . ($RowCounter + 1),qw/99$/, $LastCell);
	}
	else
	{
	$sheet1->write($RowCounter,0,$Dir,$fmtWhite); 
	$sheet1->repeat_formula($RowCounter,3,$frmlCumulativePercent,$fmtWhitePercent,qw/B2$/,'B' . ($RowCounter + 1),qw/99$/, $LastCell);
	}
	$sheet1->write($RowCounter,1,$DirsCounts{$Cnt},$fmtWhiteInt);
	$sheet1->repeat_formula($RowCounter,2,$frmlDirPercent,$fmtWhitePercent,qw/^B2$/,'B' . ($RowCounter + 1),qw/99$/, $LastCell);

	$Passed80++ if ($CumulativePercent >= .80);
	if (($Passed80 ==1)||(($RowCounter == 1) && ($Passed80 == 1)))
	{
		$sheet1->merge_range(1,4,$RowCounter ,5, "Top 80 Percent",$fmtPeach);
		$intRemainingStart = $RowCounter + 1;
	}
	$RowCounter++;
	#Sort the list of paths in "string" order.
}
$fmtPeach->set_align('top') if (($RowCounter - $intRemainingStart) > 10) ;
$sheet1->merge_range($intRemainingStart,4,$RowCounter-1 ,5,"Bottom 20 Percent",$fmtGrey);
$sheet1->set_column(0,0,$DirColumnWidth);
$sheet1->set_column(3,3,18);
#$RowCounter = 1;
#foreach my $Cnt (@DirsByCount)
#{ 
#	$sheet1->repeat_formula($RowCounter,3,$frmlDirPercent,$fmtWhitePercent,qw/^B2$/, 'B' . ($RowCounter + 1),qw/99$/, $LastCell);
#
#	$RowCounter++;
	#Sort the list of paths in "string" order.
#}
