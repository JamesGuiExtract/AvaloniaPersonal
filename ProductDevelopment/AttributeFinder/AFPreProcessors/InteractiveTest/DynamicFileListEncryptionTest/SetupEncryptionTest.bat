@echo off
del List*.etf
copy OldList1.txt.etf List1.txt.etf
attrib -r List1.txt.etf
type template.txt  > List1.txt
