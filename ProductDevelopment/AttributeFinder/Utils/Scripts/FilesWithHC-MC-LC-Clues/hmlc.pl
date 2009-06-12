#!/usr/bin/perl
# creates lists of files with HCData, MCData, LCData and Clues
# run from the OutputFiles directory

use strict;
my $dirname = $ENV{PWD};
my @contents;
my $hfile = "HCData.txt";
open HCData, ">>$hfile";
my $mfile = "MCData.txt";
open MCData, ">>$mfile";
my $lfile = "LCData.txt";
open LCData, ">>$lfile";
my $cfile = "Clues.txt";
open Clues, ">>$cfile";
#my $inputfile = "FilesWithFoundRedactions.txt";
open INFILE, "FilesWithFoundRedactions.txt" or die  $!;

while (<INFILE>){
		$_ =~ s/[\r\n]//g;
		my $imagefile = $_;
		my $voafile = $imagefile . '.voa';
		open GREPVOA, "<$voafile" or die $!;
		binmode(GREPVOA);
		my @contents=<GREPVOA>;
		close GREPVOA;
		# check for HCData, MCData, LCData, Clues
		(grep /HCData/, @contents) ? print HCData $imagefile . "\r\n" : ();
		(grep /MCData/, @contents) ? print MCData $imagefile . "\r\n" : ();
		(grep /LCData/, @contents) ? print LCData $imagefile . "\r\n" : ();
		(grep /Clues/, @contents) ? print Clues $imagefile . "\r\n" : ();
		next;
}
close HCData;
close MCData;
close LCData;
close Clues;
