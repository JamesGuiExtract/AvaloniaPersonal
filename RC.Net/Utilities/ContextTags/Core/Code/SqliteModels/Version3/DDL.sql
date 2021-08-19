--
-- File generated with SQLiteStudio v3.3.3 on Wed Aug 18 07:20:39 2021
--
-- Text encoding used: System
--
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

-- Table: Context
CREATE TABLE Context (
    ID         INTEGER        PRIMARY KEY AUTOINCREMENT
                              NOT NULL,
    Name       NVARCHAR (50)  NOT NULL
                              COLLATE NOCASE,
    FPSFileDir NVARCHAR (260) NOT NULL
                              COLLATE NOCASE
);


-- Table: CustomTag
CREATE TABLE CustomTag (
    ID   INTEGER       PRIMARY KEY AUTOINCREMENT
                       NOT NULL,
    Name NVARCHAR (50) NOT NULL
                       COLLATE NOCASE
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


-- Table: TagValue
CREATE TABLE TagValue (
    ContextID INT            NOT NULL,
    TagID     INT            NOT NULL,
    Workflow  NVARCHAR (100) DEFAULT ('') 
                             NOT NULL
                             COLLATE NOCASE,
    Value     NVARCHAR (400) NOT NULL
                             COLLATE NOCASE,
    CONSTRAINT PK_TagValue PRIMARY KEY (
        ContextID,
        TagID,
        Workflow
    ),
    CONSTRAINT FK_TagValue_Context FOREIGN KEY (
        ContextID
    )
    REFERENCES Context (ID) ON DELETE CASCADE
                            ON UPDATE NO ACTION,
    CONSTRAINT FK_TagValue_CustomTag FOREIGN KEY (
        TagID
    )
    REFERENCES CustomTag (ID) ON DELETE CASCADE
                              ON UPDATE NO ACTION
);


-- Index: Context_UC_ContextFPSFileDir
CREATE UNIQUE INDEX Context_UC_ContextFPSFileDir ON Context (
    FPSFileDir ASC
);


-- Index: Context_UC_ContextName
CREATE UNIQUE INDEX Context_UC_ContextName ON Context (
    Name ASC
);


-- Index: CustomTag_UC_CustomTagName
CREATE UNIQUE INDEX CustomTag_UC_CustomTagName ON CustomTag (
    Name ASC
);


-- Trigger: fki_TagValue_ContextID_Context_ID
CREATE TRIGGER fki_TagValue_ContextID_Context_ID
        BEFORE INSERT
            ON TagValue
      FOR EACH ROW
BEGIN
    SELECT RAISE(ROLLBACK, "Insert on table TagValue violates foreign key constraint FK_TagValue_Context") 
     WHERE (
               SELECT ID
                 FROM Context
                WHERE ID = NEW.ContextID
           )
           IS NULL;
END;


-- Trigger: fki_TagValue_TagID_CustomTag_ID
CREATE TRIGGER fki_TagValue_TagID_CustomTag_ID
        BEFORE INSERT
            ON TagValue
      FOR EACH ROW
BEGIN
    SELECT RAISE(ROLLBACK, "Insert on table TagValue violates foreign key constraint FK_TagValue_CustomTag") 
     WHERE (
               SELECT ID
                 FROM CustomTag
                WHERE ID = NEW.TagID
           )
           IS NULL;
END;


-- Trigger: fku_TagValue_ContextID_Context_ID
CREATE TRIGGER fku_TagValue_ContextID_Context_ID
        BEFORE UPDATE
            ON TagValue
      FOR EACH ROW
BEGIN
    SELECT RAISE(ROLLBACK, "Update on table TagValue violates foreign key constraint FK_TagValue_Context") 
     WHERE (
               SELECT ID
                 FROM Context
                WHERE ID = NEW.ContextID
           )
           IS NULL;
END;


-- Trigger: fku_TagValue_TagID_CustomTag_ID
CREATE TRIGGER fku_TagValue_TagID_CustomTag_ID
        BEFORE UPDATE
            ON TagValue
      FOR EACH ROW
BEGIN
    SELECT RAISE(ROLLBACK, "Update on table TagValue violates foreign key constraint FK_TagValue_CustomTag") 
     WHERE (
               SELECT ID
                 FROM CustomTag
                WHERE ID = NEW.TagID
           )
           IS NULL;
END;


COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
