#!/usr/bin/perl

# use module
#use File::Listing;
use XML::Simple;
use Data::Dumper;
use Spreadsheet::WriteExcel;
$AttributeName;

sub trim($)
{
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}
sub BuildDocsTotalFormula($)
{
	
	my $string = shift;
	my $DocsTotalFormula = '=IF(AND(J'.$string.'<>"",I'.$string.'<>""),J'.$string.'/I'.$string.',"")';
	return $DocsTotalFormula;
}
print "\nWorking...\n";
#Open and read directory
opendir (RESDIR, ".");
my  @dirlist = grep  /.+\.xml$/, readdir(RESDIR);
#print join ("\n", @dirlist);
closedir (RESDIR);
$RowCounter=1;
#create Excel File
my $workbook = Spreadsheet::WriteExcel->new("results.xls");
my $sheet1 = $workbook->add_worksheet("Test Results");
#$workbook->set_custom_color(40,'#FFFFCC');
#$workbook->set_custom_color(41,'#FFFF99');
my $format0 = $workbook->add_format();
$format0->set_bold();
$format0->set_bg_color('43');
$format0->set_align('center');
$format0->set_border();
$format0->set_num_format('0.00%');

my $format0a = $workbook->add_format();
$format0a->set_bold();
$format0a->set_bg_color('43');
$format0a->set_align('center');
$format0a->set_align('vcenter');
$format0a->set_border();
$format0a->set_num_format('0.00%');
$format0a->set_center_across();

my $format1 = $workbook->add_format();
$format1->set_bg_color(26);
$format1->set_bold();
$format1->set_align('general');
$format1->set_border();

my $format2 = $workbook->add_format();
$format2->set_border();
$format2->set_align('center');

my $format2a = $workbook->add_format();
$format2a->set_border();
$format2a->set_align('center');
$format2a->set_align('vcenter');


my $format3 = $workbook->add_format();
$format3->set_bold();
$format3->set_bg_color(43);
$format3->set_align('center');
$format3->set_border();
$format3->set_num_format('0.00');

my $format4 = $workbook->add_format();
$format4->set_bg_color(31);
$format4->set_bold();
$format4->set_border();

my $format5 = $workbook->add_format();
$format5->set_bg_color(31);
$format5->set_bold();
$format5->set_border();
$format5->set_rotation(90);
$format5->set_text_wrap();

foreach $F (@dirlist)
{
my $RegionTop = $RowCounter;
#diagnostic:
#print "File: $F\n";

# create object
#$xml = new XML::Simple(forcearray => TestCaseDetailNote, forcearray => Description, forcearray => ID,forcearray => Detail);
$xml = new XML::Simple(forcearray => TestCaseDetailNote);
# read XML file
$data = $xml->XMLin("$F");
$FileName = $F;

# diagnostic:
# print output
#print Dumper($data);
#print $data->{ComponentTest}->{TestCase};

$sheet1->activate();
@time=localtime(time);
$day=$time[3];
$month=$time[4]+1;
$year=$time[5] + 1900;
# diagnostic:
#print "Day $day Month: $month Year: $year\n"

$DatFileName = @{$data->{TestCase}}[0]->{Description}[0];
$DatFileName =~ s/.+\-(.+)\.dat/$1/;
print "Now processing $DatFileName .\n";

my $CaptureRate = $sheet1->store_formula('=IF(AND(E1<>"", D1<>"",D1<>0),E1/D1,"")'); 
my $ROCE = $sheet1->store_formula('=IF(AND(G1<>"",G1<>0, E1<>""),E1/G1,"")');
my $DocPercentage = $sheet1->store_formula('=IF(AND(J1<>"", I1<>""),J1/I1,"")');


# diagnostic:
#print "Datfile: $DatFileName\n";

foreach $e (@{$data->{TestCase}})
{	
	
	if ($e->{ID}[0]=~/\d{2}\:\d{2}\:\d{2}/)
	{
#	@testdate=split(" ", $e->{ID});
#	print join(" ", @testdate);	
	#print Dumper($e);
	foreach $td (@{$e->{TestCaseDetailNote}})
	{
		$AttributeName=trim($td->{Title});
		 $f = $td->{Detail}[0];
# diagnostic:
		#print Dumper($f);

		my @Results = ();
		foreach $g (@{$f->{Line}})
			{
# diagnostic:
#				print $g;

				push(@Results, $g);
			}
		foreach $r (@Results)
			{
			$r =~ s/.*:\s*//;
			$r =~ s/(\d+).*/$1/;
			$r =~ s/\D*//g;
# diagnostic:
#			print "Output: $r\n";

			}
		#Write column headings
		$sheet1->write(0,0,"Date",$format4);
		$sheet1->write(0,1,"Dat File",$format5);
		$sheet1->write(0,2,"Attribute Name",$format5);
		$sheet1->write(0,3,"# Attributes",$format5);
		$sheet1->write(0,4,"# Attribute Match",$format5);
		$sheet1->write(0,5,"# Attribute Match",$format5);


		$sheet1->write(0,6,"Extras",$format5);
		$sheet1->write(0,7,"ROCE",$format5);
		$sheet1->write(0,8,"# Docs",$format5);
		$sheet1->write(0,9,"# Docs Match",$format5);
		$sheet1->write(0,10,"# Docs Match",$format5);
		$sheet1->write(0,11,"Time in Seconds",$format4);

#		$sheet1->write($RowCounter,0,"$month/$day/$year",$format2);
#		$sheet1->write($RowCounter,1,$DatFileName,$format2);
		$sheet1->write($RowCounter,2,$AttributeName,$format1);
		$sheet1->write($RowCounter,3,$Results[0],$format2);
		$sheet1->write($RowCounter,4,$Results[1],$format2);
		$sheet1->repeat_formula($RowCounter,5,$CaptureRate,$format0,qw/^E1$/,'E'.($RowCounter + 1),qw/^D1$/,'D'.($RowCounter + 1),qw/^D1$/,'D'.($RowCounter + 1), qw/^E1$/,'E'.($RowCounter + 1),qw/^D1$/,'D'.($RowCounter + 1)); 
		$sheet1->write($RowCounter,6,$Results[2],$format2);
		$sheet1->repeat_formula($RowCounter,7,$ROCE,$format3,qw/^E1$/,'E'.($RowCounter + 1),qw/^G1$/,'G'.($RowCounter + 1),qw/^G1$/,'G'.($RowCounter + 1),qw/^E1$/,'E'.($RowCounter + 1),qw/^G1$/,'G'.($RowCounter + 1)); 
 		
# diagnostic:
#		print "about to increment\n";

		$RowCounter++;
	}
	}
#		$sheet1->write($RowCounter,0,"$month/$day/$year",$format2);
#		$sheet1->write($RowCounter,1,$DatFileName,$format2);

}
my @Summary;
foreach $sumline (@{$data->{Summary}[0]->{Line}})
	{
		push (@Summary, $sumline);
# diagnostic:
#		print "Output $sumline \n";

		
	} 

	foreach $s (@Summary)
		{
		$s =~ s/.*:\s*//;
		$s =~ s/(\d+).*/$1/;
		$s =~ s/\D*//g;
# diagnostic:
#		print "Output: $s\n";

		}

#No longer needed since merging:
#	for (my $i = $RegionTop; $i < $RowCounter; $i++)
#		{
#		$sheet1->write_blank($i, 8, $format2);
#		$sheet1->write_blank($i, 9, $format2);
#		$sheet1->write_blank($i, 10, $format0);
#		$sheet1->write_blank($i, 11, $format2);
#		}	
# diagnostic:
#		print join ("\n", @Summary);

#	$sheet1->write(($RowCounter-1),8,$Summary[0],$format2);
#	$sheet1->write(($RowCounter-1),9,$Summary[1],$format2);

# diagnostic:
#print "RegionTop:$RegionTop\nRowCounter$RowCounter\nDatFile$DatFileName\n";
	
# If $RowCounter wasn't incremented, increment it.
	$RowCounter++ if ($RowCounter == $RegionTop);
	if (($RowCounter - $RegionTop)>1)
	{
	$sheet1->merge_range($RegionTop , 0, $RowCounter - 1  , 0, "$month/$day/$year",$format2a);
	$sheet1->merge_range($RegionTop , 1, $RowCounter  - 1 , 1, trim($DatFileName),$format2a);
	$sheet1->merge_range($RegionTop , 8, $RowCounter - 1, 8, $Summary[0],$format2a);
	$sheet1->merge_range($RegionTop , 9, $RowCounter - 1, 9 ,$Summary[1], $format2a);
	$sheet1->merge_range($RegionTop , 10, $RowCounter - 1, 10 ,BuildDocsTotalFormula($RegionTop + 1), $format0a);
	$sheet1->merge_range($RegionTop , 11, $RowCounter - 1, 11 ,$Summary[3], $format2a);
	}
	else
	{
	$sheet1->write($RowCounter - 1  , 0, "$month/$day/$year",$format2);
	$sheet1->write($RowCounter  - 1 , 1, trim($DatFileName),$format2);
	$sheet1->write($RowCounter - 1, 8, $Summary[0],$format2);
	$sheet1->write($RowCounter - 1, 9 ,$Summary[1], $format2);
	$sheet1->write($RowCounter - 1, 10 ,BuildDocsTotalFormula($RegionTop + 1), $format0);
	$sheet1->write($RowCounter - 1, 11 ,$Summary[3], $format2);
	}

#	$sheet1->repeat_formula(($RowCounter -1),10,$DocPercentage,$format0,qw/^J1$/,'J'.($RowCounter),qw/^I1$/,'I'.($RowCounter),qw/^J1$/,'J'.($RowCounter),qw/^I1$/,'I'.($RowCounter));
#	$sheet1->write(($RowCounter-1),11,$Summary[3]." seconds",$format2);
}
print "Results will be found in results.xls\n";
