#!/usr/bin/perl

# use module
use XML::Simple;
use File::Copy;

sub trim($)
{
	my $string = shift;
	$string =~ s/^\s+//;
	$string =~ s/\s+$//;
	return $string;
}

#Open and read directory
opendir (RESDIR, ".");
my  @dirlist = grep  /.+\.xml$/, readdir(RESDIR);
#print join ("\n", @dirlist);
closedir (RESDIR);
foreach $F (@dirlist)
{
#print "File: $F\n";
# create object
$xml = new XML::Simple;
# read XML file
$data = $xml->XMLin("$F");
$FileName = $F;

my $NewFileName = @{$data->{TestCase}[0]->{TestCaseFile}}[1];

#This should get the directory name
$NewFileName =~ s/\\[^\\]+$//;
$NewFileName =~ s/.+\\//;
#This should "CamelCase" the directory name
$NewFileName =~ s/\bof\b/Of/;
$NewFileName =~ s/\s//g;
if ($ARGV[1] ne "")
{
	$NewFileName = $ARGV[0] . $NewFileName;
	$NewFileName .= $ARGV[1];
	$NewFileName .= ".xml";
}
else
{
	$NewFileName .= $ARGV[0] ;
	$NewFileName .= ".xml";
}
print "Copying $F to $NewFileName\n";
copy($F, $NewFileName) or die "It didn't work\n" ;
}
