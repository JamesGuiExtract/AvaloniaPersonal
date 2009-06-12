#!/bin/sh

#get_EAVs v2
#
#
#	HOW TO USE(TYPICALLY):
#	-------------------------------------------------------------------------
#		** all are files are one entry per line
#	setnames	-> contains the names of the sets sampled : Set001, Set002, etc.
#	attribnames	-> contains the names of the attribute folders that comprise a single set
#	filenames	-> contains the names of the files that are to comprise the sample
#	-------------------------------------------------------------------------

#	CAVEAT: setnames, attribnames, filenames and this script 
#			MUST ALL BE IN THE PARENT DIRECTORY OF THE SETS
#				i.e.: the directory that is most likely named after the 
#					client and has all the set directories visible
#	-------------------------------------------------------------------------

if [ $1 != "" ]; then
	Sample="$1";
	else Sample="Sample";
fi

#count=0;

if [ ! -e "$Sample" ]; then
	echo "mkdir: $Sample";
	mkdir $Sample;	fi
if [ ! -e "$Sample/EAV" ]; then
	echo "mkdir: $Sample/EAV/";
	mkdir $Sample/EAV;	fi
for y in `cat attribnames`; do
	if [ ! -e "$Sample/EAV/$y" ]; then
		echo "mkdir: $Sample/EAV/$y";
		mkdir "$Sample/EAV/$y";	fi
done;		
	
for y in `cat attribnames`; do
	for z in `cat setnames`; do
		for x in `cat filenames`; do 
				if [ -e "$z/EAV/$y/$x" -a ! -e "$Sample/EAV/$y/$x" ]; then
#					count=$((count + 1));
					echo -n "cpEAV: "; 
#					echo -n $count;
					echo " $Sample/EAV/$y/$x";
					cp $z/EAV/$y/$x $Sample/EAV/$y/;
				fi					
		done; 
	done; 
done;
