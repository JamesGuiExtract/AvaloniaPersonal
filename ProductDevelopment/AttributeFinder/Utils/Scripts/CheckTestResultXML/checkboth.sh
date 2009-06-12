#!/usr/bin/bash
grep -P  "<TestCase>[\s\S]+?</TestCase>" $1 \
|grep -P "Expected\sAttributes[\s\S]+?</TestCaseMemo>|\.uss|<Title>\s+Found\sAttributes[\s\S]+?</TestCaseMemo>|FAILURE|SUCCESS"\
| sed -e "/</d" \
|sed -e "s/.\+\.uss/\n&/" \
