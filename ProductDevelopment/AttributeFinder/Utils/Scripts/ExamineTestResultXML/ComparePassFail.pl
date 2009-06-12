#!/usr/bin/perl

# use modules
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

# create object
$xml = new XML::Simple;
my @Results;
my @PassedFirstFailedSecond;
my @FailedFirstPassedSecond;
my @PassedBoth;
my @FailedBoth;
# read first XML file
$data = $xml->XMLin($ARGV[0]);
foreach $e (@{$data->{TestCase}})
{	
	unless ($e->{ID}=~/\s200\d/)
	{
		my $filename = trim(${$e->{TestCaseFile}}[1]);
		if ($e->{TestCaseResult} =~/FAIL/)
		{	
			
			#print "Failed file: $filename\n";
			push (@Results, [$filename,0]);
			
		}
		elsif ($e->{TestCaseResult} =~/SUCC/)
		{	
			#print "Passed file: $filename\n";
			push (@Results, [$filename,10]);
		}
	
	}
}



# read second XML file
$data = $xml->XMLin($ARGV[1]);

my $i = 0;
foreach $e (@{$data->{TestCase}})
{	
	unless ($e->{ID}=~/\s200\d/)
	{
		my $filename = trim(${$e->{TestCaseFile}}[1]);
		if ($filename eq @Results[$i]->[0])
		{
			if ($e->{TestCaseResult} =~/SUCC/)
			{	
				
				@Results[$i]->[1] += 1;

				
			}
			elsif ($e->{TestCaseResult} =~/FAIL/)
			{	
				
				@Results[$i]->[1] += 0;

				
			}
			else
			{
				die "Something went wrong\n";
			}
		}
	
		else 
		{	
		die "Filename1:$filename\nFilename2:$Results[$i]->[0]\n";
		
		}
	$i++;
	}
}
foreach $res (@Results)
{
	my $Result = $res->[1] + 0;
	if ($Result == 11)
	{
		push (@PassedBoth, $res->[0]);
	}
	elsif ($Result == 0)
	{
		push (@FailedBoth, $res->[0]);
	}
	elsif ($Result == 10)
	{
		push (@PassedFirstFailedSecond, $res->[0]);
	}
	elsif ($Result == 1)
	{
		push (@FailedFirstPassedSecond, $res->[0]);
	}
	else
	{
		print "Something went wrong\n";
	}
}
print "First test: $ARGV[0]\n";
print "Second test: $ARGV[1]\n\n";
print "Files passing the first test and failing the second test:\n";
foreach $element  (@PassedFirstFailedSecond)
{
	print "$element\n";
}
print "\nFiles passing the second test and failing the first test:\n";
foreach $element  (@FailedFirstPassedSecond)
{
	print "$element\n";
}
print "\nFiles failing both tests:\n";
foreach $element  (@PassedBoth)
{
	print "$element\n";
}
print "\nFiles passing both tests:\n";
foreach $element  (@FailedBoth)
{
	print "$element\n";
}

print "\nTotals:\n";
my $Total;
$Total = scalar(@PassedBoth);
print "Passed Both:\t\t\t$Total\n";
$Total = 0;
$Total = scalar(@FailedBoth);
print "Failed Both:\t\t\t$Total\n";
$Total = 0;
$Total = scalar(@PassedFirstFailedSecond);
print "Passed First, Failed Second:\t$Total\n";
$Total = 0;
$Total = scalar(@FailedFirstPassedSecond);
print "Passed Second, Failed First:\t$Total\n";
$Total = scalar(@Results);
print "Total files: $Total\n";
