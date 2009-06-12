#!/bin/sh

#PARTIAL COMMAND TO RUN (filled in at the next step)
runs_on="ls ";

### DETERMINE IF USER WANTS TO RUN ON ALL FILES IN THIS DIRECTORY, 
	## OR HAS SPECIFIED A SINGLE FIlE
if [ "$1" != "-v" -a "$1" != "" ];	
	then	runs_on+="$1";
	else	runs_on+="*.xml";	
fi
	
#MAKE THE EAV DIRECTORY, if it does not exist
if [ ! -e ../../EAV/ ];	then	mkdir ../../EAV;	fi
	
		
#THE LOOP
################
for file in `$runs_on`; do

#	OPTIONAL VERBOSE OUTPUT MODE
	if [ "$1" == "-v" ];	then	echo "file->  $file";	fi
	
	
###########################################
#####	GET FILE NAME TO USE	
	line=`egrep -o '<InputFile>.*</InputFile>' $file`;
	junk='</InputFile>';
	new_file=${line%*$junk};
	new_file=${new_file##*'\'};
	new_file+=.eav;
	
	
#	CHECK for existance of the file, prompt user if necessary
	if [ -e ../../EAV/$new_file ];
		then
			#PROMPT USER
			##################################################################
			while [ "$ans" != "y" -a "$ans" != "n" -a "$ans" != "a" -a "$ans" != "z" ]
			do	
				echo "  *** file $new_file already exists!! ***";
				echo -n "OVERWRITE THIS FILE? [Yes/No/All/Zero]: "; read ans;
			done
			#If 1-time overwrite, remove file and set user-answer var to null
			if [ "$ans" == "y" ]; then	rm ../../EAV/$new_file;	ans="";	fi
			#If overwrite all, remove file and do not reset user's answer	
			if [ "$ans" == "a" ]; then	rm ../../EAV/$new_file; fi			
	fi		
			
				
	## CREATE FILE
	##################################################################		
	if [ "$ans" != "n" ]; then

		if [ ! -e ../../EAV/$new_file ]; 
		then
			#####	GET & OUTPUT CLUE (if it exists in file)
			if [ "`egrep -o 'Redaction\ Type=\"Clue\"\ Output=\"[01]\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file`" != "" ]; 
				then 	
				egrep -o 'Redaction\ Type=\"Clue\"\ Output=\"[01]\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file |
					sed -e 's/<Zone[^>]\+><\/Line><Line>/\\r\\n/' | egrep -o ">[^<]+<" | egrep -o "[^<>]+" | 
					sed -e 's/^/Clues|/' | sed -e 's/$/|SSN\r/' >> ../../EAV/$new_file;
			elif [ "`grep -o Clue $file`" != "" ]; then 
			 	egrep -o "Redaction Type=\"Clue\" Output=\"[01]\"><Line>[^<]+<Zone[^>]+></Line>" $file | egrep -o ">[^<]+<" | 
			 		egrep -o "[^<>]+" | sed -e 's/^/Clues|/' | sed -e 's/$/|SSN\r/' >> ../../EAV/$new_file;
			fi
			
			#####	GET & OUTPUT DOCTYPE (if it exists in file)
			if [ "`grep -o DocType $file`" != "" ]; then 			
				echo -n "DocumentType|" >> ../../EAV/$new_file;
				egrep -o '<DocType>.*</DocType>' $file| egrep -o ">[^<]+<" | egrep -o "[^<>]+" |
					sed -e 's/$/\r/' >> ../../EAV/$new_file;
			fi

			#####	GET & OUTPUT Redaction data (if it exists in file)
			#################################################################
			###  CONDITIONALS FOR HCDATA			
			if [ "`egrep -o 'Redaction\ Type=\"High\"\ Output=\"1\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file`" != "" ]; 
				then 	
				egrep -o 'Redaction\ Type=\"High\"\ Output=\"1\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file |
					sed -e 's/<Zone[^>]\+><\/Line><Line>/\\r\\n/' | egrep -o ">[^<]+<" | egrep -o "[^<>]+" | 
					sed -e 's/^/HCData|/' | sed -e 's/$/\/\/|SSN\r/' >> ../../EAV/$new_file;
		
			elif [ "`grep -o 'Redaction\ Type=\"High\"\ Output=\"1\"' $file`" != "" ]; then 
			 	egrep -o "Redaction Type=\"High\" Output=\"1\"><Line>[^<]+<Zone[^>]+></Line>" $file | egrep -o ">[^<]+<" | 
			 		egrep -o "[^<>]+" | sed -e 's/^/HCData|/' | sed -e 's/$/\/\/|SSN\r/' >> ../../EAV/$new_file;
			fi
				
			###  CONDITIONALS FOR LCDATA
			if [ "`egrep -o 'Redaction\ Type=\"Low\"\ Output=\"1\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file`" != "" ]; 
				then 	
				egrep -o 'Redaction\ Type=\"Low\"\ Output=\"1\"><Line>[^<]+<Zone[^>]+></Line><Line>[^<]+<Zone' $file |
					sed -e 's/<Zone[^>]\+><\/Line><Line>/\\r\\n/' | egrep -o ">[^<]+<" | egrep -o "[^<>]+" | 
					sed -e 's/^/LCData|/' | sed -e 's/$/\/\/|SSN\r/' >> ../../EAV/$new_file;
						
			elif [ "`grep -o 'Redaction\ Type=\"Low\"\ Output=\"1\"' $file`" != "" ]; then 
			 	egrep -o "Redaction Type=\"Low\" Output=\"1\"><Line>[^<]+<Zone[^>]+></Line>" $file | egrep -o ">[^<]+<" | 
			 		egrep -o "[^<>]+" | sed -e 's/^/LCData|/' | sed -e 's/$/\/\/|SSN\r/' >> ../../EAV/$new_file;
			fi	
			
			###  clause for MANUAL REDACTION ENTRY
#			if [ "`grep -o 'Redaction\ Type=\"Man\"\ Output=\"1\"' $file`" != "" ]; then 
#			 	egrep -o "Redaction Type=\"Man\" Output=\"1\"><Line>[^<]+<Zone[^>]+></Line>" $file | egrep -o ">[^<]+<" | 
#			 		egrep -o "[^<>]+" | sed -e 's/^/Man|/' | sed -e 's/$/\r/' >> ../../EAV/$new_file;
#			fi			
			###  clause for MANUAL REDACTION ENTRY
			if [ "`grep -o 'Redaction\ Type=\"Man\"\ Output=\"1\"' $file`" != "" ]; then 
			 	egrep -o "Redaction Type=\"Man\" Output=\"1\"><Line>[^<]+<Zone[^>]+></Line>" $file | 
			 	sed -e 's/<Zone[^P]\+/ /' | egrep -o ">[^<]+<" | 
			 		egrep -o "[^<>]+" | sed -e 's/^/ManualEntry|/' | sed -e 's/$/\r/' >> ../../EAV/$new_file;
			fi						
		fi
				
	#RESET USER'S ANSWER IN THE CASE OF 1-TIME NON-OVERWRITE RESPONSE
	else ans="";
	fi
	
done