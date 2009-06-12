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
# read first XML file
print "Files with no found attributes:\n";
$data = $xml->XMLin($ARGV[0]);
foreach $e (@{$data->{TestCase}})
{	
	unless ($e->{ID}=~/\s200\d/)
	{
		my $filename = trim(${$e->{TestCaseFile}}[2]);
		if ($e->{TestCaseException}->{Text} =~/No\s*Found/)
		{	
			
			print "$filename\n";
			
		}
	
	}
}



