--
-- File generated with SQLiteStudio v3.3.3 on Wed Aug 18 07:17:10 2021
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: FPSFile
CREATE TABLE FPSFile (
    ID                     INTEGER        PRIMARY KEY AUTOINCREMENT
                                          NOT NULL,
    FileName               NVARCHAR (512) 
                                          COLLATE NOCASE,
    NumberOfInstances      INT            DEFAULT (1) 
                                          NOT NULL,
    NumberOfFilesToProcess INT            DEFAULT ( -1) 
                                          NOT NULL
);


-- Table: Settings
CREATE TABLE Settings (
    Name  NVARCHAR (100) NOT NULL
                         COLLATE NOCASE,
    Value NVARCHAR (512) 
                         COLLATE NOCASE,
    CONSTRAINT PK_Settings PRIMARY KEY (
        Name
    )
);


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
