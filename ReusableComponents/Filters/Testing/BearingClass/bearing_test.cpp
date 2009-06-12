//**** test driver, main.cpp file****
//Copyright 1998 Uclid Software, LLC
#pragma warning( disable : 4786 )

#include "bearing.h"

void help_message();                 //instruction 
void test_from_keyboard();           //get bearing string or radians from keyboard
void test_from_file(string strOption, char* fileName);   //get bearing string or radians from file

void get_bearing();          //get bearing from keyboard
void get_radians();          //get radians from keyboard
void test_bearing(Bearing  bearingB, string strText); //test bearing which get from keyboard
void test_radians(Bearing  bearingB, double angle);   //test radians which get from keyboard
bool compart_vec(vector<string> vec_1,   vector<string>  vec_2); //vec_1 <= vec_2, return true
void print_vector(ofstream& fout, vector<string> vec);  //print the vector

//****************************************************************************************
//Main function
//****************************************************************************************
int main(int argc, char*argv[])
{
 
 
	cout<<endl<<"************    Test bearing class    ************"<<endl;

	string command;   //"keyboard", "-b", "-r" are valid command

	if(argc ==1)    //no command
	{
		help_message();   
		exit(0);
	}
	if(argc ==2)   //command must be "keyboard", otherwise invalid
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
	else if(argc == 3)  //command must be "-b" or "-r", otherwise invalid
	{
		command = argv[1];
		if(command == "-b" || command == "-r")
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
		cout<<"There options for testing the Bearing class."<<endl;
		cout<<"1) Test bearing string from the file."<<endl
			<<"   Type .exe file followed by option '-b' and the input file name.\n"
			<<"   example  ...> bearing_test -b t1.txt "<<endl;
		cout<<"2) Test radians from the file."<<endl
			<<"   Type .exe file followed by option '-r' and the input file name.\n"
			<<"   example  ...> bearing_test -r t2.txt "<<endl;
		cout<<"3) Test bearing string or radians from key board."<<endl
			<<"   Type .exe file followed by ' -k '\n"
			<<"   example  ...> bearing_test -k "<<endl;
}
//---------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------
//Test input from keyboard, prompt user to choose option for testing bearing string or radians
void test_from_keyboard()
{
	char iOption;
	//test bearing string and radians from key board
	while(1)
	{
		cout<<" **** Test bearing string or radians from key board ****"<<endl;
		cout<<" Press 1, get input string from keyboard."<<endl;
		cout<<" Press 2, get input radians from keyboard."<<endl;
		cin>>iOption;
		if(iOption == '1' || iOption == '2')
			break;
		else
			cout<<"Invalid choice, please enter your choice again"<<endl;
	}

   	switch(iOption)
	{
		case '1':
			get_bearing();
			break;
		case '2':
			get_radians();
			break;
		default:
			cout<<"Not valid choice"<<endl;
	}
}	 
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
//Get bearing string from keyboard. Since i use getline(cin....), may be some thing wrong in
//library, user must hit "Enter" twice.
void get_bearing()
{
	//get input string from keyboard
	cout<<"  **** TEST bearing string, get input string from key board ****"<<endl;

	string strStop = "stop";
	string strChangeMode = "reverse";
	while(1)
	{
		Bearing bearingB1;
		string  strText;
		cout<<"Please enter the string, *press Enter twice*\n"
			<<"(Type stop will stop the test. "
			<<" Type reverse will change reverse movd)"<<endl;
		 
		cin.ignore(80, '\n');
		getline(cin, strText, '\n');
		if(strText == strStop)
			break;
		if(strText == strChangeMode)
			bearingB1.toggleReverseMode();
		else
			test_bearing(bearingB1, strText);
		 
	}
}
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
//Get radians from keyboard. then call test_radians() to get the result
void get_radians()
{
	//get input radians from keyboard
	cout<<"  **** TEST radians, get input radians from key board ****"<<endl;

	while(1)
	{
		Bearing bearingB4;
		double angle;
		cout<<"Please enter the radians.\n"
			<<"(Type -111 will stop the test. "
			<<" Type -11 will change reverse movd)"<<endl;
		 
		cin >> angle;
		 
		if(angle == -111)
			break;
		if(angle == -11)
			bearingB4.toggleReverseMode();
		test_radians(bearingB4, angle);	 
	}
}
//----------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------
//Test bearing string, call evaluate(..) function, cout result
void test_bearing(Bearing  bearingB, string strText)
{
	cout<<"    Input string    \""<<strText<<"\""<<endl;
	 
	
  
	//asString() and getDegrees() have throw string exception
	try
	{
		bearingB.evaluate(strText.c_str());
		cout<<"                    \""<<bearingB.asString()<<"\""<<endl;
		cout<<"                     "<<bearingB.getDegrees()<<endl;
	}
	catch(const string& strExcept)
	{
		cout<<"                    "<<strExcept<<endl;
	}
 
	//if is not valid, maybe has some alternate string
	if(!bearingB.isValid())
	{
		vector<string> vec_string;
		vec_string = bearingB.getAlternateStrings();
		vector<string>::iterator theIterator;
		 
		for(theIterator = vec_string.begin(); theIterator != vec_string.end(); 
			theIterator++)
			{
				cout<<"         Alternate  \"" << *theIterator<<" \""<<endl;
			}
	}
	
}
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
//Test radians, call evaluateRadians(double)
void test_radians(Bearing bearingB, double angle)
{
	cout<<"    Input radians   \""<<angle<<"\""<<endl;
	

	try
	{
		bearingB.evaluateRadians(angle);
	}
	catch(const string& strExcept)
	{
		cout<<"                    "<<strExcept<<endl;
	}
	try
	{
		cout<<"                    \""<<bearingB.asString()<<"\""<<endl;
		cout<<"                     "<<bearingB.getDegrees()<<endl;
	}
	catch(const string& strExcept)
	{
		cout<<"                    "<<strExcept<<endl;
	}
}
//-----------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
//Get bearing string or radians from file.
//If option is "-b", open bearing string file and write the result to "out1.txt".
//If option is "-r", open radians file and write the result to "out2.txt".
//Print out the summary
//In result file, there are two types FAIL, one is invalid string or invalid radians,
//the other is not match the standard string or not match angle
void test_from_file(string strOption, char *fileName)
{
	ifstream fin;
	ofstream fout;
	 
	fin.open(fileName);
	
	if(strOption == "-b")
	{
		cout<<"  **** TEST , get input bearing from file  ****"<<endl;
		cout<<"  Result is in out1.txt"<<endl;
		fout.open("out1.txt");
	}
	else if(strOption == "-r")
	{
		cout<<"  **** TEST , get input radians from file  ****"<<endl;
		cout<<"  Result is in out2.txt"<<endl;
		fout.open("out2.txt");
	}
	else  //invalid option
		exit(1);


	int iNumTest =  0;            //number of test string or radians
	int iNumPass =  0;            //number of valid string or radians   (suppose file is correct)
	int iNumFail =  0;            //number of invalid string or radians (suppose file is correct)
	int iBadString = 0;           //invalid string;
	int iGoodString = 0;          //valid string

	//this two variable help us check which part not match
	int iNoMatchString = 0;    //number of not match the standard string which file give us
	int iNoMatchAngle =0;      //number of not match the angle which file give us
	int iNoMatchAltStr = 0;
	if(fin.fail())
	{
		cerr<<"!!!! Error open file, abort"<<endl;
		exit(1);
	}

	while(!fin.eof())
	{
		

		//file should be four parts 1)input string, 2)standard string, 3)angle string
		//4)alternate string
		string strInput = "";
		string strStand = "";
		string strAngle = "";
		string strAlt   = "";           //alternate string
		string strAlternate = "";
		vector<string> vec_alt_str;      //vector hold alternate string

		//creat bearing object
		Bearing bearingB2;

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

		//get the angle string
		while(fin.get(ch))
		{
			if( ch != ',' && ch != '\n')
			{
				if(!isspace(ch))
					strAngle += ch;
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
				if(strAlt == "NONE")
					strAlternate = strAlt; 

				vec_alt_str.push_back(strAlt);
				strAlt = "";
				if(ch == '\n' || ch == EOF)
					break;
			}
		}

 

		//if strOption is "-b", evaluate the bearing string
		//if strOption is "-r", evaluate the radians, when evaluate the radians, if the radians
		//value is not valid (<0 or > 2*PI), the efvaluatRadians(dValue) will throw a exception
		 
		if(strInput != "")  //avoid empty line
		{
			iNumTest++;    //number of line for testing
			if(strOption == "-b")
				bearingB2.evaluate(strInput.c_str());
			else if(strOption == "-r")
				bearingB2.evaluateRadians(atof(strInput.c_str()));
			else  //not valid option
				exit(1);

			//check string
			if(!bearingB2.isValid())  //bearing string is not valid
			{
				//iNumFail++;
				iBadString++;
				fout<<strInput;
				fout<<"     Invalid string  ";

				vector<string> vec_string;
				vec_string = bearingB2.getAlternateStrings();
				bool compare = true;

				//if field in file is NONE, do not check, otherwise check
				if(strStand != "NONE")
				{
					fout<<" **Fail no standard string, should be \"NONE\" ";
					iNoMatchString++;
					iNumFail++;
				}
				else if(strAngle != "NONE")
				{
					fout<<" **Fail no angle value, should be \"NONE\" ";
					iNoMatchAngle++;
					iNumFail++;
				}
			 
				//suggest alternate string must in bearing alternate vector
				//get the Alternate string
				//vec_string is vector from the bearing class
				//vec_alt_str is vector from test file
				//vec_alt_str <= vec_string will be fine
				
				else if(strAlternate != "NONE" && !compart_vec(vec_alt_str, vec_string))
				{
					compare = false;
					iNumFail++;
					iNoMatchAltStr++;
					
					fout<<endl<<"       **FAIL Alternate string not match *"<<endl;
					//fout alternate in file and alternate calculated
					fout<<"        Alternate string in file    = ";     
					print_vector(fout, vec_alt_str);
					fout<<"        Alternate string calculated = ";
					print_vector(fout, vec_string);

				}

				if(strStand == "NONE" && strAngle == "NONE" && compare)
				{
					iNumPass++;
				 
					fout<<"    * PASS, bad string *"<<endl;
				}
		 
			}
			else  //string is valid, check if it match the file
			{
				iGoodString++;
				fout<<strInput;				
				if(strStand != "NONE" && strStand != bearingB2.asString())  //valid, then check match standard string
				{
					iNumFail++;
					iNoMatchString++;  //not match the standard string which file give us.
	
					 
					fout<<endl<<"    **FAIL (not match standard string)"<<endl;
					fout<<      "    Standard string in file    = "<<strStand<<endl;
					fout<<      "    Standard string calculated = "<<bearingB2.asString()<<endl;

				}else  //match stand string, keep checking angle
				{
					double dTemp1 = bearingB2.getDegrees();
					double dTemp2 = atof(strAngle.c_str());


					//compare two double value.  
					if(strAngle != "NONE" && fabs(dTemp1 - dTemp2)>0.02)
					{
						iNumFail++;
						iNoMatchAngle++; //not match the angle which the file give us

						 
						fout<<endl<<"    **FAIL (not match the angle)"<<endl;
						fout<<      "    Angle string in file    = "<<strAngle<<endl;
						fout<<      "    Angle string calculated = "<<bearingB2.getDegrees()<<endl;

					}
					else //match the angle, check alternate string should be standard string
					{
						vector<string> vec_string;
						vec_string = bearingB2.getAlternateStrings();
			 
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
							//fout<<strInput;
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
	cout<<"  Total Not match the Angle              "<<iNoMatchAngle<<endl;
	cout<<"  Total Not match the Alternate string   "<<iNoMatchAltStr<<endl;
}

//first vector is from file, second is from bearing,
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


 

