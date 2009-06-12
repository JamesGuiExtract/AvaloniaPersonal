#!/usr/bin/perl
# run from the directory with the file lists you wish to search
# with the test result XML as the first argument e.g.,
#
# getlistinfo.pl EXTRACT-longFileName\ -\ 16.39.32.xml
# 
# output from above example would be in EXTRACT-longFileName\ -\ 16.39.32.dat

use strict;
my $dirname = $ENV{PWD};

my @xmlfile;
my $outputfile = $ARGV[0];
$outputfile =~ s/\.xml/\.dat/i;
open OUTFILE, ">$outputfile";

while (<>){
		my $line = $_;
		# don't output lines ending in dat uss voa
		(! grep /(?:^<|\.(?:dat|uss|voa).?$)/i, $line) ? print OUTFILE $line : ();
		# add blank line after SUCCESS | FAILURE
		(grep /(?:SUCCESS|FAILURE)/, $line) ? print OUTFILE "\n" : ();
		# search for image names in all txt files
		if (grep /\.(?:tiff?|bin|pdf|[0-9]{3}).?$/i, $line) {
			opendir(DIR, $dirname) or die "can't opendir $dirname: $!";
			my @filelist = readdir DIR;
			foreach (@filelist){
				my $filename = $_;
				if (grep /\.txt.?/, $filename){
				open FILE, "<$filename" or die "can't open $filename : $!";
					while (<FILE>){
						( $line eq $_ ) ? print OUTFILE "$filename\n" : ();
					}
				}		
				close FILE;
			}	
			closedir(DIR);
		}
		#output contents of nte files after .nte lines
		if (grep /\.nte.?$/i, $_) {
			$_ =~ s/\\/\//g;
			$_ =~ s/^\([a-z]\):/\/cygdrive\/$1/g;
			open NTE, "<$_" or next;
			my @nte = <NTE>;
			close NTE;			
			print OUTFILE "@nte\n";
		}
		next;
}

close OUTFILE;
