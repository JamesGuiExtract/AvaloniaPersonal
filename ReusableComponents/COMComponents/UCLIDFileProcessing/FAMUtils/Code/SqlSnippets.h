#pragma once
#include <string>

namespace SqlSnippets {
	static const std::string CREATE_ALLINDEXES_TEMP_TABLE =
		" IF OBJECT_ID('tempdb..#AllIndexes') IS NOT NULL DROP TABLE #AllIndexes; \r\n"
		" \r\n"
		" CREATE TABLE #AllIndexes( \r\n"
		" 	INDEX_NAME VARCHAR(MAX) NOT NULL, \r\n"
		" 	COLUMNS VARCHAR(MAX) NOT NULL, \r\n"
		" 	TABLE_NAME VARCHAR(MAX) NOT NULL) \r\n"
		" \r\n"
		" --Get all indexes from the database \r\n"
		" INSERT INTO \r\n"
		" 	#AllIndexes(INDEX_NAME, COLUMNS, TABLE_NAME) \r\n"
		" SELECT \r\n"
		" 	sys.indexes.[name] \r\n"
		" 	, substring(column_names, 1, len(column_names) - 1) \r\n"
		" 	, schema_name(sys.objects.schema_id) + '.' + sys.objects.[name] \r\n"
		" FROM \r\n"
		" 	sys.objects \r\n"
		" 		INNER JOIN sys.indexes \r\n"
		" 			ON sys.objects.object_id = sys.indexes.object_id \r\n"
		" \r\n"
		" 			CROSS APPLY( \r\n"
		" 				SELECT \r\n"
		" 					col.[name] + ', ' \r\n"
		" 				FROM \r\n"
		" 					sys.index_columns \r\n"
		" 						INNER JOIN sys.columns col \r\n"
		" 							ON sys.index_columns.object_id = col.object_id \r\n"
		" 							AND sys.index_columns.column_id = col.column_id \r\n"
		" 				WHERE \r\n"
		" 					sys.index_columns.object_id = sys.objects.object_id \r\n"
		" 					AND \r\n"
		" 					sys.index_columns.index_id = sys.indexes.index_id \r\n"
		" 				ORDER BY \r\n"
		" 					key_ordinal FOR XML PATH('')) D(column_names) \r\n"
		" WHERE \r\n"
		" 	sys.objects.is_ms_shipped < > 1 \r\n"
		" 	AND \r\n"
		" 	index_id > 0; \r\n";
}
