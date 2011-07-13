#pragma warning(disable: 4786)

#include"ELIChecker.h"

#include <windows.h>

// windows.h must be included before cppUtil.h
#include <cppUtil.h>

short main(int iargc, char* pszTestCasesFile[])
{
	if(iargc==2)
	{
		//Required arguments supplied.
	}

	else if( iargc > 2 )
		{
			cout<<"Too many arguments supplied.\n";
			cout<<"The Syntax is:\n\n";
			cout<<"ELIChecker [ InputTextfile ]\n";
			return 0;
		}
		else if(iargc<2)
			{
				cout<<"One more argument expected.\n";
				cout<<"The Syntax is:\n\n";
				cout<<"ELIChecker [ InputTextfile.txt ]\n";
				return 0;
			}

	/*This is my input file object. you know,
	Input text file contains all exe files names including paths. 
	Using this file object, I'll open Input text file and read the exe files names.*/
	ifstream InputFile;
	/*
	Here once I'm creating and closing this output text file. Otherwise if file already exist our code appends data to the file.
	Why I'm opening file in append mode, you will see in ELIFinder() function.
	*/

	string strELIFileName = "ELI.txt";
	string strELIFilePath = tmpnam(strELIFileName);

	string strDuplicateELIFileName = "DuplicateELI.txt";
	string strDuplicateELIFilePath = tmpnam(strDuplicateELIFileName);

	ofstream output(strELIFilePath.c_str());
	output.close();
	waitForFileAccess(strELIFilePath, giMODE_READ_ONLY);

	//I'm creating a string object to store exe file name read from Input text file.
	string strfile_ExeFileName;
	

	int iBinaryFilesCount = 0, 
		iELITotal = 0,
		iELIDuplicate = 0;

	/*Here I'm opening Input text file and checking for the 
	ELIFinder() will open the exe file and finds ELI codes in exe file. 
	it will store all ELI codes in character array and returns the char pointer
	this pointer represents the base address of the ELI codes*/

	InputFile.open(pszTestCasesFile[1]);

	//If File not opened, give message.....
	if(!InputFile.is_open())
	{
		cout<<"InputFile Not Opened. Please check the File.\n";
		exit(0);
	}
	

	//Start Reading Input File...
	while(getline(InputFile,strfile_ExeFileName))//So you got the Exe name with path in this function.
	{
		//Here I'm getting ELI count from each binary file and calculating 
		//Total ELI codes found in all exe files.
		
		iELITotal = iELITotal+ ELIFinder(strfile_ExeFileName);
		//Counting the binary files.
		iBinaryFilesCount++;
	}
	InputFile.close();

	cout<<"ELI codes writing operation completed successfully \n";
	iELIDuplicate = ELICompare();
	
	

	ifstream file_Details(strDuplicateELIFilePath.c_str());
	string strDetails;

	/*Here I'm displaying detail information about Duplicate ELI codes. 
	For that I'm creating again one file pointer and reading each line and displaying on console.*/
	
	cout<<"\n===================\n";
	cout<<"\nDuplicates Details:\n===================\n";

	//Reading duplicates details and displaying on console.
	
	while(getline(file_Details,strDetails))
	{
		cout<<strDetails<<endl;
	}

	file_Details.close();
	InputFile.close();
	
	cout<<"\nTotal Binary Files Processed     : "<<iBinaryFilesCount;
	cout<<"\nTotal ELI Codes Processed        : "<<iELITotal;
	cout<<"\nTotal duplicate ELI codes found  : "<<iELIDuplicate;
	cout<<"\nTotal unique ELI codes found     : "<<iELITotal-iELIDuplicate<<endl;
	

	try
	{
		deleteFile((LPCTSTR)strELIFilePath.c_str(), false, false);
	}
	catch (...)
	{
		cout<<"\n Unable to delete temporary file : "<< strELIFilePath;
	}

	try
	{
		deleteFile((LPCTSTR)strDuplicateELIFilePath.c_str(), false, false);
	}
	catch (...)
	{
		cout<<"\n Unable to delete temporary file : "<< strDuplicateELIFilePath;
	}

	if(iELIDuplicate>0)
	{
		return EXIT_FAILURE;
	}
	else
		return EXIT_SUCCESS;
}



/*
	This function will not take any parameters but returns number of Duplicate ELI codes found.
	This function will do the following operation:
	1.  Opens ELI.txt file that was written by ELIFinder() function.
	2.  Reads Each Exe Name and ELI codes followed by the Exe name.
	3.  Compares Each ELI code with other ELI codes and identifies duplicates.
	4.  Writes All Duplicate ELI codes to the text file.
	5.  Returns number of duplicate ELI codes.	

*/
int ELICompare()
{
	string strELIFileName = "ELI.txt";
	string strELIFilePath = tmpnam(strELIFileName);

	string strDuplicateELIFileName = "DuplicateELI.txt";
	string strDuplicateELIFilePath = tmpnam(strDuplicateELIFileName);	

	//Creating file pointer and opening ELI.txt for reading Exe names and ELI codes.
	ifstream file_CompareInput(strELIFilePath.c_str());
	//Creating file stream and opening Op.txt for writing Duplicate ELI codes
	ofstream file_DuplicateELI(strDuplicateELIFilePath.c_str());  

	//Here I'm storing ELI codes which are identified as having duplicates.
	vector<string>	Duplicates;
	
	string	strExeFileName1, //This will hold exe file name.
		strELICode1, //This will hold ELI code.
		strTemp="";	
	
	int	 iPosition = 0,		//This is my current file pointer position.
		iDuplicateELI = 0,	//Duplicate ELI count.
		iMultiTimesInOneFile = 0,	//Multile ELI count in same exe file
		iMTimesCount=0;		//This says there are duplicates for current ELI code.
	bool bResult = false,	//This tells me that I'm taking First Input ELI code which is already identified as duplicate.
		bMultipleOne = false, //This tells me that current ELI is duplicate one.
		bFirstDuplicate = false; //I will know when first duplicate is identified.
	
	//Read FirstInput Exe name.
	while(getline(file_CompareInput,strExeFileName1) && (strExeFileName1.find("ELI",0)))
	{
				
		static int iLinecount = 0; //To avoid already compared ELI codes. With this count, I'll skip ILineCount numbers of lines while reading the file.
		iLinecount++;

		//Read FirstInput ELI code. If u find ELI in string then only it's and ELI. Otherwise it is ExeName.
		while(getline(file_CompareInput,strELICode1) && (strELICode1.find("ELI",0)==0))
		{		
			bFirstDuplicate = true;//because this is first record. So, make it true.
			
			vector<string>::iterator vectBegin = Duplicates.begin();
			vector<string>::iterator vectEnd   = Duplicates.end();
			//Initially vector is empty. When ever we find Duplicate ELI codes, we will fill this Vector.
			//After reading first input ELI code, I'll compare with Vector elements(Which are already Identified Duplicate ELIs)
			//If this ELI is duplicate of any element, then I'll go back to while loop otherwise, continue the process.
			
			for(; vectBegin != vectEnd; ++vectBegin)
			{				
				strTemp = *vectBegin;//Get Vector Element.

				if(strTemp==strELICode1)//Compare ELI with Vector element.
				{
					bResult = true;
					iLinecount++;
				}
				if(bResult)
					break;
			}
			
			
			//This is to avoid already compared ELI codes next time. 
			//For Example I already compared Exe1.ELI1 with Exe2.ELI4. 
			//Next time whenever I compare Exe2.ELI4 with others, I need not compare Exe1.ELI1.
			//For this I'm keeping count of already compared lines and skipping next time.
			int iInnercount = iLinecount; 
			if(bResult)
			{				
				bResult = false;
				continue;
			}
			string strExeFileName2, strELICode2;//To hold Second Exe Name and ELI code.
						
			fstream file_CompareWith(strELIFilePath.c_str());//Open ELI.txt when contains list of binary (Exe or Dll) File Names and ELI codes in it.
			
			//file_CompareInput.seekg(file_CompareWith.tellg());

			//Read second binary file name.			
			while(getline(file_CompareWith,strExeFileName2) && (strExeFileName2.find("ELI",0)))
			{
				//Skip all those llines which already compared.
				for(int j =0; j <iInnercount; j++)
				{
					getline(file_CompareWith,strELICode2);
				}

				//Read Second Input ELI code.
				while(getline(file_CompareWith,strELICode2) && (strELICode2.find("ELI",0)==0))
				{
					
					//So, now you have both ELI codes. now compare them.
					if(strELICode1.compare(strELICode2)==0)
					{
						//Increase duplicate ELI count.
						iDuplicateELI++; 
						bMultipleOne = true;
						iMTimesCount++;//I need this count because I there is only one duplicate, I'll say 1 time. If I found more than one duplicate, I'll say 2times,3times and so on.
						
						//In case of Duplicates found within one binary file,
						if(strExeFileName1==strExeFileName2)
						{		
							//Increase count, to trak how many times same ELI code is repeated in one Binary file.
							iMultiTimesInOneFile++;
						}
						
					}
					
					
				}

				//In case withing one file repeated ELI codes found
								
				if((strExeFileName1==strExeFileName2) &&(iMultiTimesInOneFile>0))
				{
				
					file_DuplicateELI<<"\n"<<strELICode1<<endl;
//					cout<<"\n"<<strELICode1<<endl;
					file_DuplicateELI<<strExeFileName1<<"\t\t"<<iMultiTimesInOneFile+1<<"time"<<"\n";
//					cout<<strExeFileName1<<"\t\t"<<iMultiTimesInOneFile+1<<"times"<<"\n";
					iMultiTimesInOneFile=0;
					iMTimesCount=0;
				
				}
				else
				{
					//Here I'm writing Duplicate ELI codes, Exe Names and number of times found in each file.
					//I'm writing all this data to c:\DupliatesELI.txt file.
					//If duplicates are found for the current ELI code, then I need to write ExeName1 also in the list. 
										
					if(bFirstDuplicate && iMTimesCount>0)
					{
						file_DuplicateELI<<"\n"<<strELICode1<<endl;
						file_DuplicateELI<<strExeFileName1<<"\t\t"<<"1 time(s)"<<"\n";
						bFirstDuplicate = false;
					}
					
					
					if(iMTimesCount>0 && iMTimesCount<2)
					{												
						file_DuplicateELI<<strExeFileName2<<"\t\t"<<"1 time(s)"<<"\n";
						iMTimesCount = 0;
					}
					else 
						if(iMTimesCount>1)
						{						
							file_DuplicateELI<<strExeFileName2<<"\t\t"<<iMTimesCount<<"times"<<"\n";
//							cout<<strExeFileName2<<"\t\t"<<iMTimesCount<<"times"<<"\n";
							iMTimesCount = 0;
						}
				}

				//You know, in while loop,
				//you already read one line in advance 
				//(this is not wrong because, after reading this line you come to know that no more ELI codes in this Exe file).
				//So go backto the previous line and get ELI code.

				iPosition = file_CompareWith.tellg();
				file_CompareWith.seekg(iPosition - (strELICode2.size()+2));
				iInnercount = 0;
				
			}
			
			
			file_CompareWith.close();
			iLinecount++;
			
			//Here I'm writing duplicate ones in to the vector.

			if(bMultipleOne)
			{
				Duplicates.push_back(strELICode1);
				bMultipleOne = false;
			}
		}

		//You know, in while loop,
		//you already read one line in advance 
		//(this is not wrong because, after reading this line you come to know that no more ELI codes in this Exe file).
		//So go backto the previous line and get ELI code.
		
		iPosition = file_CompareInput.tellg();
		file_CompareInput.seekg(iPosition - (strELICode1.size()+2));
		
	}
	
	file_CompareInput.close();
	file_DuplicateELI.close();
	waitForFileAccess(strDuplicateELIFilePath, giMODE_READ_ONLY);
	
	return iDuplicateELI;
	
}




/******************************************************************************************************/
/******************************************************************************************************/



/*This function takes Exe file name as a parameter of type string. 
This function will do the following operations:

1. Opening the Exe file in binary mode. Exe File name is supplied as a parameter to this func()
2. Converts string type File name to Char ptr. Because file open function doesn't supports 
   string objects as parameters. Here  function will get char pointer to this strfile_ExeFileName string obj.
3. After this  file_ExeFile is opened using this char ptr(contains file_ExeFileName address).
4. Now function will get each line in the binary file and checks for ELI codes in file.
*/

int ELIFinder(string strfile_ExeFileName)
{
	ifstream file_ExeFile;     //Here I'm delaring file stream for Input file. 
	const char* pszExeName;	  // Why this char pointer is declared you know? File file stream not taking string object as file name. What to do? I'm delcaring char pointer and converting string to char pointer and passing this pointer.
	int iELITotal = 0,  //This Integer will count Total ELI codes read from binary files.
	    iPosition = 0;  //This Integer will hold the starting position of "ELI" in strings. I'll read 8 characters as ELI code starting from this position.

	string	strFileData,  //This string object is to hold file data for a while while reading file line by line. 
			strElicode ;  //This string object will hold ELI code.

	pszExeName = strfile_ExeFileName.c_str();//Here we are converting string object to char pointer object.

	/*
	Here I'm opening Exe file in Binary mode. Iinitially I opened this file with default mode(Text mode).
	I got some problems. File stream reading only five lines of text and coming out.
	Later I Updated open function with ios::binary parameter. Now all lines read properly till EOF.
	*/
	file_ExeFile.open(pszExeName,ios::binary); 
	
	if(!file_ExeFile.is_open())
	{
		cout<<pszExeName<<"  File Not Opened. Please check the File.\n";

	}

	string strELIFileName = "ELI.txt";
	string strELIFilePath = tmpnam(strELIFileName);
	
	ofstream output(strELIFilePath.c_str(),ios_base::app);//I'm writing all ELI codes to C:\ELI.txt .
	
	output<<strfile_ExeFileName<<"\n";//All ELI codes and Exe Names I'm writing to the text file. File: C:\op.txt

	while(!file_ExeFile.eof())//Here I'm check till not end of file, I'll cintinue reading the data.
	{
		
		//file_ExeFile>>strFileData;
		getline(file_ExeFile,strFileData);//Reading line by line from the input file stream.

		while((iPosition = strFileData.find("ELI"))>0) //Here I'm getting the starting position of "ELI" in string object.
		{
			/*We know that ELI code size is 8 characters length.
			I have the position where ELI starts. I can read substring(8 characters length) from this postion which is required ELI code.
			*/
			strElicode = strFileData.substr(iPosition,8);	//m_pPosition is your starting position of ELI code. So read 8 characters from this start point.
			iELITotal++;										//Here I'm incrementing the Total count of ELIs read.
			
			output<<strElicode<<"\n";//Writing ELI code to the ELI.txt file.
			/* 
			I have a case like same string object having three ELI codes.
			so while searching for ELI code, it's always returning first ELI code.
			I don't how many ELI codes existing in the string.
			So after reading ELI I'm replacing ELI with UMR. Now if I search for next ELI, it will
			show exactly next one. Is there any thing wrong in the process I followed?
			*/
			strFileData.replace(iPosition,3,"UMR");//Here I'm renaming ELI to UMR. Otherwise each time finding the same ELI code.
			
		}
	
	}

	//Letus tell the user on console.
	cout<<"ELI Codes written successfully for "<<strfile_ExeFileName<<"\n";

	file_ExeFile.close();//Se we already read all ELI codes in this Exe file. We can close file now.
	output.close();//We already written all ELI codes to the text file. I can close the text file now.
	waitForFileAccess(strELIFilePath, giMODE_READ_ONLY);
	
	return iELITotal;

}


string tmpnam(string strELIFileName)
{
	char strTempDirPath[MAX_PATH];
		
	// getting the path for the Temp folder
	GetTempPath(MAX_PATH,(LPTSTR)(LPCTSTR)strTempDirPath);
	
	// final file name with its full path
	string strTempFileName = strTempDirPath + strELIFileName;

	return strTempFileName;
}