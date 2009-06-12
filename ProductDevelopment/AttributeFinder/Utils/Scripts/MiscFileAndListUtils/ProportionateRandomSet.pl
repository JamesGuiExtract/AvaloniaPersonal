#!/usr/bin/perl -w


use strict;
#Do we need current working directory?
use Cwd;
my $SampleSize;
# Default to tif.
my $ImageFileExt = "tif";
my @DirList;
#my @Counts;
#my @ImageLists;
my %DirsCounts;
my %DirsPaths;
my %RandomDirsPaths;
my @arrCommentLines;
my @arrExcluded;
my $boolVerboseOutput = 0;

if ($ARGV[0] && ($ARGV[0] /1 > 0))
{
	$SampleSize = $ARGV[0];
	print "\nSample Size: $SampleSize\nCreateSampleSet.bat will be created in the current directory.\n";
}
else
{
die "\nUsage:\nProportionateRandomSet.pl <SampleSize> [optional: <file extension>] [optional:  -v  for verbose output]\nDirectories to be excluded may be listed in a file called exclude.list in the current directory\n";
#$SampleSize = 1000;
}
$ImageFileExt = $ARGV[1] if (($ARGV[1]) && !($ARGV[1] eq "-v"));
if (($ARGV[1] eq "-v") || (($ARGV[1] ne "-v") &&  (defined $ARGV[2]) && ($ARGV[2] eq "-v")))
{
	print "\nVerbose output:\n";
	$boolVerboseOutput = 1;
}
#die "The 'uss' file extension is not reasonable.\n" if($ImageFileExt eq "uss");
print "File Extension: $ImageFileExt\n";
if (-f "./exclude.list")
{
	open (EXC, "./exclude.list");
	@arrExcluded = <EXC>;
	close EXC;
}
#from http://www.perl.com/doc/FAQs/FAQ/oldfaq-html/Q4.13.html
sub round {
    my($number) = shift;
    return int($number + .5);
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
my $boolExclude = 0;
foreach (@arrExcluded)
{
	chomp $_;
#	print "Element: $_\n";
	my $strCompareFDir = $FDir;
	$strCompareFDir =~ s/^\.\///;
	$boolExclude = 1 if ($_ eq $strCompareFDir);
	print "Excluding directory: $FDir\n" if ($boolExclude == 1);
}
#print "FDir: $FDir\n";
if ($boolExclude == 0)
	{
		my @TempImageArray;
		opendir(DIR, "$FDir");
		@TempImageArray = grep(/\.$ImageFileExt$/i,readdir(DIR));
		closedir(DIR);
		#older code using arrays
		#push (@Counts, $TempDirCount);
		#push (@ImageLists, @TempImageArray);
		$DirsCounts{$FDir}=scalar(@TempImageArray);
		$DirsPaths{$FDir}=\@TempImageArray;
	}
}

#Old Code using arrays
#@Counts and @DirList should line up unless there was a problem.
#for (my $i = 0; $i < scalar(@Counts);$i++)
#{
#	print "Directory: $DirList[$i], Count $Counts[$i]\n";
#}
my $Total = 0;

foreach my $key (keys (%DirsCounts))
{
#	print "$DirsCounts{$key}\n";
	$Total += $DirsCounts{$key};
}
die "No .$ImageFileExt files found.\n" if ($Total == 0);
die "\nSample size: $SampleSize\ncannot be greater than or equal to\n directorytotal $ImageFileExt files: $Total\n" unless ($SampleSize < $Total);
#	print "Element: $_\n";
print "\nTotal $ImageFileExt Files: $Total \n";

# sort DirsCounts by Value, then reverse sort  by key
my @DirsByCount = reverse(sort { $DirsCounts{$a} <=> $DirsCounts{$b} || $b cmp $a} keys %DirsCounts);
my $RunningTotal = 0;
my $Once = 0;
foreach my $Cnt (@DirsByCount)
{
	
	# Multiply by 10000, round, divide by 100 to get percentage to 2 places.

	my $Percentage = round(($DirsCounts{$Cnt}/$Total)*10000)/100;
	

#	my $Percentage = $DirsCounts{$Cnt}/$Total;
	$RunningTotal += $Percentage;
#	my $RandArrayLength = 10;
	my $RandArrayLength = round($DirsCounts{$Cnt}/$Total * $SampleSize);
	my $BigArrayLength = $DirsCounts{$Cnt};
	
	print "\nProcessing directory: $Cnt\n";
	push ( @arrCommentLines , "rem Directory: $Cnt\n");
	print "Number of $ImageFileExt Files: $BigArrayLength\n"; 
	push ( @arrCommentLines , "rem Number of  $ImageFileExt files: $BigArrayLength \n");
	print "Percentage of total: $Percentage \n";
	push ( @arrCommentLines , "rem Percentage of total: $Percentage \n");
	print "Picking random $RandArrayLength of $BigArrayLength files.\n";
	push ( @arrCommentLines , "rem Sample: $RandArrayLength of $BigArrayLength files.\n\n");
	
#	die $BigArrayLength;
#	print "Calculated: $RandArrayLength Count". scalar(@{$DirsPaths{$Cnt}}) . "\n";
#	Lengths should be ok, but if not:
#	die "Issue with counts\n" if !(round(scalar(@{$DirsPaths{$Cnt}})) == $RandArrayLength);
#	Set every element of TempPickedArray to 0.
	my @TempPickedArray;
	for (my $i = 0;$i<$BigArrayLength;$i++)
	{
		$TempPickedArray[$i] = 0;
	}	
	my @TempPathsArray;
	until (scalar(@TempPathsArray) == $RandArrayLength)
	{
		my $PathIndex = int(rand($BigArrayLength));
		if ($TempPickedArray[$PathIndex] == 0)
		{ 
			push (@TempPathsArray,  $DirsPaths{$Cnt}[$PathIndex]);
			$TempPickedArray[$PathIndex] = 1;
			print "Picked element $PathIndex of $BigArrayLength\n" if ($boolVerboseOutput == 1);
			
		}
		
	}
	#Sort the list of paths in "string" order.
	@TempPathsArray = sort {$a cmp $b} @TempPathsArray;
	$RandomDirsPaths{$Cnt} = \@TempPathsArray;

#	print statement to check
#	foreach my $element (@TempPathsArray)
#	{
#		print "Random Image:". $Cnt . "/". $element. "\n";
#	}


#	print "Debug: $Cnt\n";
}# end  foreach my $Cnt (@DirsByCount)

open(OUTFILE, ">./CreateSampleSet.bat");
print  OUTFILE "\@echo off\n";
print  OUTFILE "rem Batch file to create random sample set\n";
print  OUTFILE "rem Sample set information:\n";
foreach (@arrCommentLines)
{
	print OUTFILE $_;
}
print OUTFILE "rem First create RandomSubset directory\n";
my $SubsetPath = "..\\Random" . $SampleSize . "Subset";
#print OUTFILE 'mkdir "'. $SubsetPath .'"' . "\n";
print OUTFILE qq(mkdir "$SubsetPath"\n);
print OUTFILE "rem  Then create subdirectories: \n";
# Make the subdirectories
foreach my $Cnt (@DirsByCount)
{
	# Remove dot-slash from $Cnt
	my $DirName = $Cnt;
	$DirName =~ s/\.\///; 
	print OUTFILE qq(mkdir "$SubsetPath\\$DirName"\n);
}
print OUTFILE qq(rem Now copy the images:\n);
foreach my $Cnt (@DirsByCount)
{
	my $DirName = $Cnt;
	$DirName =~ s/\.\///; 
	foreach my $Image (@{$RandomDirsPaths{$Cnt}})
	{
		$Image =~ s/\.uss$// if ($ImageFileExt eq "uss");
		#First copy the tif
		print  OUTFILE qq(copy ".\\$DirName\\$Image" "$SubsetPath\\$DirName\\"\n);
		#Then the uss
		print  OUTFILE qq(copy ".\\$DirName\\$Image.uss" "$SubsetPath\\$DirName\\"\n);
	}
}
close (OUTFILE);
