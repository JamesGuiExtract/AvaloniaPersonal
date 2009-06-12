#!/usr/bin/bash
grep -P  "<TestCase>[\s\S]+?</TestCase>" $1 \
|grep -P "Expected\sAttributes[\s\S]+?</TestCaseMemo>|\.uss|<Title>\s+Found\sAttributes[\s\S]+?</TestCaseMemo>|FAILURE|SUCCESS"\
| sed -e "/</d" \
|sed -e "s/.\+\.uss/\nSTARTPOINT\n&/" \
|sed -e "s/$/<NL>/" \
| perl -p -e "s/\n//" \
| sed -e "s/STARTPOINT/\n/g" \
|grep SUCCESS \
|sed -e "s/\(<NL>\)\+/\n/g" \
|sed -e "/^$/d" \
|sed -e "s/SUCCESS/&\n/"
