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
$xml = new XML::Simple(ForceArray => 1);
# read first XML file
$data = $xml->XMLin($ARGV[0]);
foreach $e (@{$data->{TestCase}})
{	
	unless ($e->{ID}=~/\s200\d/)
	{
		print "TestCase:\n" . trim(${$e->{TestCaseFile}}[2])."\n";
		print trim(${$e->{TestCaseFile}}[3])."\n\n";
	foreach $m (@{$e->{TestCaseMemo}}) 
	{
		print  trim($m->{Title}[0]) . ":\n";
		foreach $n (@{$m->{Detail}})
		{
			foreach $o (@{$n->{Line}})
			{
				print trim($o)."\n";
			}
		}
		print "\n";
	}
	
	}
}
