//**** test driver, main.cpp file****
//Copyright 1998 Uclid Software, LLC
#pragma warning( disable : 4786 )
#include <distance.h>

void help_message();
void test_from_keyboard();           //get distance string or radians from keyboard
void get_distance();           //for keyboard
void test_distance(Distance  distanceD, string strText); //for keyboard

void test_from_file(string strOption, char* fileName);   //get distance string from file
bool compart_vec(vector<string> vec_1,   vector<string>  vec_2); //vec_1 <= vec_2, return true
void print_vector(ofstream& fout, vector<string> vec);  //print the vector

//****************************************************************************************
//Main function
//****************************************************************************************
int main(int argc, char*argv[])
{
	//print message
	cout<<endl<<"****    Test distance class    ****"<<endl;

	string command;   //"-k", "-d", are valid command

	if(argc ==1)    //no command
	{
		help_message();   
		exit(0);
	}
	if(argc ==2)   //command must be "-k", otherwise invalid
	{
	    command = argv[1];
		if(command == "-k")
			test_from_keyboard();
		else
		{
			cout<<"Invalid command"<<endl;
			help_message();
			exit(0);
		}
	}
	else if(argc == 3)  //command must be "-d", otherwise invalid
	{
		command = argv[1];
		if(command == "-d" )
			test_from_file(command, argv[2]);
		else
		{
			cout<<"Invalid command"<<endl;
			help_message();
			exit(0);
		}
	}
	else
	{
		cout<<"Invalid command"<<endl;
		help_message();
		exit(0);
	}

	return 0;
}
	
//**********************************  End Main  ****************************************

//--------------------------------------------------------------------------------------
//Print instruction, tell user how to run this program
void help_message()
{
	//print help message
		cout<<endl<<"**** Help Message ****"<<endl<<endl;
		cout<<"Two options for testing the Distance class."<<endl;
		cout<<"1) Test distance string from the file."<<endl
			<<"   Type .exe file followed by option '-d' and the input file name.\n"
			<<"   example  ...> distance_test -d t1.txt "<<endl;
		 
		cout<<"2) Test distance string from key board."<<endl
			<<"   Type .exe file followed by ' -k '\n"
			<<"   example  ...> distance_test -k "<<endl;
}
//---------------------------------------------------------------------------------------

//Test input from keyboard,  
void test_from_keyboard()
{
	cout<<" **** Test distance string from key board ****"<<endl;
	get_distance(); 
 
}	 
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
//Get distance string from keyboard. Since i use getline(cin....), may be some thing wrong in
//library, user must hit "Enter" twice.
void get_distance()
{
	//get input string from keyboard
	string strStop = "stop";
	while(1)
	{
		Distance distanceD1;
		string  strText;
		cout<<"Please enter the string, *press Enter twice* (type stop will stop the test)"<<endl;
		getline(cin, strText, '\n');
		cin.ignore(80, '\n');
		if(strText == strStop)
			break;
		test_distance(distanceD1, strText);
		 
	}
}

//----------------------------------------------------------------------------------------
//Test distance string, call evaluate(..) function, cout result
void test_distance(Distance  distanceD, string strText)
{
	string strBaseUnit = "foot";
	cout<<"    Input string    \""<<strText<<"\""<<endl;
	try
	{ //cout<<"hi"<<endl;
		distanceD.evaluate(strText.c_str());
  
		//asString() and getDegrees() have throw string exception
	
		cout<<"                    \""<<distanceD.asString()<<"\""<<endl;
		cout<<"                     "<<distanceD.getDistance(strBaseUnit.c_str())
			<<" (feet)"<<endl;
	}
	catch(const string& strExcept)
	{
		cout<<"                    "<<strExcept<<endl;
	}
 
	//if is not valid, maybe has some alternate string
	if(!distanceD.isValid())
	{
		vector<string> vec_string;
		vec_string = distanceD.getAlternateStrings();
		vector<string>::iterator theIterator;
		 
		for(theIterator = vec_string.begin(); theIterator != vec_string.end(); 
			theIterator++)
			{
				cout<<"         Alternate  \"" << *theIterator<<" \""<<endl;
			}
	}
	
}


//-----------------------------------------------------------------------------------------
//Get distance string from file.
//If option is "-d", open distance string file and write the result to "out1.txt".
//Print out the summary
//In result file, there are two types FAIL, one is invalid string or invalid radians,
//the other is not match the standard string or not match angle
void test_from_file(string strOption, char *fileName)
{
	ifstream fin;
	ofstream fout;
	 
	fin.open(fileName);
	
	if(strOption == "-d")
	{
		cout<<"  **** TEST , get input distance from file  ****"<<endl;
		cout<<"  Result is in out1.txt"<<endl;
		fout.open("out1.txt");
	}
	else  //invalid option
		exit(1);


	int iNumTest =  0;            //number of test distance
	int iNumPass =  0;            //number of pass
	int iNumFail =  0;            //number of fail
	int iBadString = 0;           //invalid string;
	int iGoodString = 0;          //valid string

	//this three variable help us check which part not match
	int iNoMatchString = 0;    //number of not match the standard string which file give us
	int iNoMatchDistance =0;      //number of not match the angle which file give us
	int iNoMatchAltStr = 0;
	if(fin.fail())
	{
		cerr<<"!!!! Error open file, abort"<<endl;
		exit(1);
	}

	string strBaseUnit = "foot";
	while(!fin.eof())
	{
		

		//file should be four parts 1)input string, 2)standard string, 3)distance(foot)
		//4)alternate string
		string strInput = "";
		string strStand = "";
		string strDistance = "";
		string strAlt   = "";           //alternate string
		string strAlternate = "";
		vector<string> vec_alt_str;      //vector hold alternate string

		//creat bearing object
		Distance distanceD2;

		unsigned char ch;
		//get the input radians 
		while(fin.get(ch))
		{
			if(ch != ',' && ch != '\n')
				strInput += ch;
			else 
				break;
		}

		//get the stand string
		while(fin.get(ch))
		{
			if(ch != ',' && ch != '\n')
			{
				if(!isspace(ch))
					strStand += ch;
			}
			else 
				break;
		}

		//get the distance(foot) string
		while(fin.get(ch))
		{
			if( ch != ',' && ch != '\n')
			{
				if(!isspace(ch))
					strDistance += ch;
			}
			else 
				break;
		}
		//get alternate string, put them to vector
	 
		while(fin.get(ch))
		{
			 
			if(ch != '|' && ch != '\n' && ch != EOF)
			{
				if(!isspace(ch))
					strAlt += ch;
			}
			else
			{
				//push string to vector
				//cout<<"^^^^ string "<<strAlt<<endl;
				vec_alt_str.push_back(strAlt);
				strAlt = "";
				if(ch == '\n' || ch == EOF)
					break;
			}
		}

 

		//evaluate the distance string
		 
		if(strInput != "")  //avoid empty line
		{
			iNumTest++;    //number of line for testing
			if(strOption == "-d")
				distanceD2.evaluate(strInput.c_str());
			else  //not valid option
				exit(1);

			//check string
			if(!distanceD2.isValid())  //distance string is not valid
			{
				//iNumFail++;
				iBadString++;
				fout<<strInput;
				fout<<"     Invalid string  ";

				vector<string> vec_string;
				vec_string = distanceD2.getAlternateStrings();
				bool compare = true;

				//if field in file is NONE, do not check, otherwise check
				if(strStand != "NONE")
				{
					fout<<" **Fail no standard string, should be \"NONE\" ";
					iNoMatchString++;
					iNumFail++;
				}
				else if(strDistance != "NONE")
				{
					fout<<" **Fail no angle value, should be \"NONE\" ";
					iNoMatchDistance++;
					iNumFail++;
				}
				//suggest alternate string must in distance alternate vector
				//get the Alternate string
				//vec_string is vector from the distance class
				//vec_alt_str is vector from test file
				//vec_alt_str <= vec_string will be fine
				if(!compart_vec(vec_alt_str, vec_string))
				{
					compare = false;
					iNumFail++;
					iNoMatchAltStr++;
					fout<<strInput;
					fout<<"       **FAIL Alternate string not match *"<<endl;
					//fout alternate in file and alternate calculated
					fout<<"        Alternate string in file    = ";     
					print_vector(fout, vec_alt_str);
					fout<<"        Alternate string calculated = ";
					print_vector(fout, vec_string);
				}
				if(strStand == "NONE" && strDistance == "NONE" && compare)
				{
					iNumPass++;
					 
					fout<<"    * pass, bad string *"<<endl;
				}
		 
			}
			else  //string is valid, check if it match the file
			{
				iGoodString++;
				fout<<strInput;					
				if(strStand != "NONE" && strStand != distanceD2.asString())  //valid, then check match standard string
				{
					iNumFail++;
					iNoMatchString++;  //not match the standard string which file give us.
	
					fout<<endl<<"    **FAIL (not match standard string)"<<endl;
					fout<<      "    Standard string in file    = "<<strStand<<endl;
					fout<<      "    Standard string calculated = "<<distanceD2.asString()<<endl;

				}else  //match stand string, keep checking angle
				{
					double dTemp1 = distanceD2.getDistance(strBaseUnit.c_str());
					double dTemp2 = atof(strDistance.c_str());

					//compare two double value.  
					if(strDistance != "NONE" && fabs(dTemp1 - dTemp2)>0.02)  
					{
						iNumFail++;
						iNoMatchDistance++; //not match the distance which the file give us

						fout<<strInput;
						fout<<endl<<"    ** FAIL (not match distance value )"<<endl;
						fout<<      "    Distance value in file  =   "<<dTemp2<<endl;
						fout<<      "    Distance value calculated = "<<dTemp1<<endl;
						 
					}
					else //match the angle, check alternate string should be standard string
					{
						vector<string> vec_string;
						vec_string = distanceD2.getAlternateStrings();
			 
						if(strAlternate != "NONE" && !compart_vec(vec_alt_str, vec_string))
						{
							iNumFail++;
							iNoMatchAltStr++;
							
							fout<<endl<<"    **FAIL ( not match alternate string )"<<endl;
							fout<<      "    Alternate string in file    = ";     
							print_vector(fout, vec_alt_str);
							fout<<      "    Alternate string calculated = ";
							print_vector(fout, vec_string);
						}
						else
						{
							iNumPass++;
							 
							fout<<"    * pass *"<<endl;
						}
					}
				}
			}
		}	
		fout<<endl;
	}

	//test summary
	cout<<"  Test summary:"<<endl;
	cout<<"  Total tests executed: "<<iNumTest<<endl;
	cout<<"  Total tests passed:   "<<iNumPass<<endl;
	cout<<"  Total tests failed:   "<<iNumFail<<endl<<endl;
	cout<<"  Total Not match the standard string    "<<iNoMatchString<<endl;
	cout<<"  Total Not match the distance           "<<iNoMatchDistance<<endl;
	cout<<"  Total Not match the Alternate string   "<<iNoMatchAltStr<<endl;
}


//first vector is from file, second is from distance,
//vec_1 <= vec_2 will be fine
bool compart_vec(vector<string> vec_1,  vector<string> vec_2)
{
	bool bFlag = false;
 
	vector<string>::iterator theIterator1;
	vector<string>::iterator theIterator2;
	 
	for(theIterator1 = vec_1.begin(); theIterator1 != vec_1.end(); theIterator1++)
	{
		bFlag = false;
	 
		for(theIterator2 = vec_2.begin(); theIterator2 != vec_2.end(); theIterator2++)
		{
		 
			if(*theIterator1 == *theIterator2)
				bFlag = true;
		}
		if(!bFlag)
			return false;	 
	}
	 
	return true;
}


void print_vector(ofstream& fout, vector<string> vec)
{
	vector<string>::iterator theIterator;
	for(theIterator = vec.begin(); theIterator != vec.end(); theIterator++)
		fout<<"  "<<*theIterator;
	fout<<endl;
}



 