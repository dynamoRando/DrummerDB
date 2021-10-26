# Overview
This document describes the overall Page architecture.  

A page is a byte array of size defined in `Constants.cs` file. It is the smallest unit of storage in the database system.

Generally speaking, pages do not get deleted, but the data that is on them can. The only time a page truly gets deleted is when all the data on the page is removed, and the database is rebuilt (i.e. all logical deletions are actually removed from the database). This does not reset the max page id for each type.

This means, for example, that you could have a page where there is no logical data on it - all the rows on a page have been deleted, but the page still takes up space on disk to keep the page ordering intact.

The smallest possible database size is the size of a Page, which would normally be just a database file consisting only of 1 System Page. This occurs when someone first creates a database but no other structures have been added (tables, contracts, indexes, etc.)

### Logical versus Actual 
In general, when there are deletions to data, the data is _logically_ deleted, meaning that it is flagged for deletion. The data on a page only gets removed when the database or page is _rebuilt_. 

The reason for logical deletions versus actual deletions is the expense in computational time to re-order the database file and all associated pointers to the rows. This concept also applies to counts, such as Total Rows versus Total Logical Rows - the total row count includes the total number of rows (including ones that have been deleted) to allow for consistency in row access in arrays, while the logical row count is the number of actual rows the database system recognizes.

### Rebuilds
When a database or page is _rebuilt_, all logical actions are executed and counts are reset depending on the level the rebuilt is executed against. This means that rows that are flagged for deletion are actually removed from the database, and Row Counts (and Ids) are reset to their actual values. 

Rebuilding a database is an expensive operation in terms of time and computation, and when a database is being rebuilt, all pending operations are halted until the rebuild is finished.

## Page Preamble
Every page starts with two INT values -    
	- **PageId**. A unique value for the type of page to identify it from others.  
	- **PageType**. An enum defined in `PageType.cs` file.  

Beyond the initial page preamble, specific page types may have defined preambles for identifying various information about the page before storing variable length data.

## Page Types
The following types of pages are described below.

| Type        | Description                                                                                                     |
| ----------- | --------------------------------------------------------------------------------------------------------------- |
| Data        | Used in storing data for tables, etc. A data page is further divided into two different types: User and System. |
| System      | Used to store database name, version, created date, etc.                                                        |
| Index       | Used to store indexes                                                                                           |
| Contract    | Used to store contract information                                                                              |
| Participant | Used to store participant information                                                                           |
| Users       | Used to store user information                                                                                  |
| Permissions | Used to store permissions for each user                                                                         |

### Data Page Types
A data page is further divided into two other types:  

| Type   | Description                                                |
| ------ | ---------------------------------------------------------- |
| User   | Data pages created by the user, i.e. user created tables   |
| System | Data pages used by the database system, i.e. system tables |

## Data Page
The following layout applies to a data page.

| Field Name             | Section            | Data Type      | Description                                                                                      |
| ---------------------- | ------------------ | -------------- | ------------------------------------------------------------------------------------------------ |
| PageId                 | Page Preamble      | INT            | Uniquely identifies the page for this type                                                       |
| PageType               | Page Preamble      | INT            | An enum defined in `PageType.cs`                                                                 |
| TotalBytesUsed         | Data Page Preamble | INT            | The total bytes used in the page, not including the size of the full preamble                    |
| TotalRows              | Data Page Preamble | INT            | The total number of rows that are held on this page. This number includes rows that are deleted. |
| DatabaseId             | Data Page Preamble | INT            | The database id that this page belongs to. For a system data page, this defaults to 0.           |
| TableId                | Data Page Preamble | INT            | The table id that this page belongs to                                                           |
| Data Page Type         | Data Page Preamble | INT            | An enum that describes if this is a user data page or system data page, see `DataPageType.cs`    |
| Logical Storage Policy | Data Page Preamble | INT            | An enum that describes where the rows of the table will be stored                                |
| Row Data               | (Rest of Page)     | Not Applicable | The rest of the page is data belonging to the rows. For more information, see `Row.md` file.     |

### System Data Page Schema
A system data page has a schema that is hard-coded into the database system. The following describes the system data schema, broken down by table columns for each table.

#### UserTables
Table Id of 1. This system data table stores all the user defined tables in the database. 

| Column Name        | Data Type | Length | Remarks                                                                                                                                             |
| ------------------ | --------- | ------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| Table Id           | INT       | n/a    | Stores the internal table id                                                                                                                        |
| Table Name         | CHAR (50) | 50     | Stores the name of the table                                                                                                                        |
| Total Rows         | INT       | n/a    | Stores the total number of rows in the table. Note that this is different from the number of logical rows - in other words, this is the max row id. |
| Total Logical Rows | INT       | n/a    | Stores the total number of logical rows in the table. This does not include the number of deleted rows.                                             |
| Is Deleted         | BIT       | n/a    | This denotes if the table has been deleted from the database or not.                                                                                |
| User Object Id     | GUID      | n/a    | The unique identifier for this database object                                                                                                      |

#### UserTableSchemas
Table Id of 2. This table stores all the column information for each table in the database.

| Column Name         | Data Type | Length | Remarks                                                                                                                          |
| ------------------- | --------- | ------ | -------------------------------------------------------------------------------------------------------------------------------- |
| Table Id            | INT       | n/a    | This is the table that the column belongs to.                                                                                    |
| Column Id           | INT       | n/a    | This is the unique column id for the column.                                                                                     |
| Column Name         | CHAR (50) | 50     | This is the name of the column.                                                                                                  |
| Column Type         | INT       | n/a    | This is the data type of the column. Note that this should follow the `ColumnTypes` enum in the `SystemSchemaConstants.cs` file. |
| Column Length       | INT       | n/a    | This is the length of the column, if not a fixed binary width.                                                                   |
| Column Ordinal      | INT       | n/a    | This is the logical ordinal value of the column.                                                                                 |
| Column Is Nullable  | BIT       | n/a    | This denotes if the column will accept NULL values.                                                                              |
| Column Binary Order | INT       | n/a    | This is the binary sort order for the column, and the order it will appear in the byte array format.                             |
| User Object Id      | GUID      | n/a    | The unique identifier for this database object                                                                                   |

#### UserObjects

Table Id of 3. This table stores all the user objects defined in the database.

| Column Name | Data Type | Length | Remarks                                                         |
| ----------- | --------- | ------ | --------------------------------------------------------------- |
| ObjectId    | CHAR      | 36     | The GUID of the object                                          |
| ObjectType  | INT       | n/a    | An enum described in `SystemSchemaConstants.cs` for ObjectTypes |
| ObjectName  | CHAR      | 50     | The object's name                                               |


## System Page
A system page is always the first page in a data file. It contains information about the the database. There is only 1 system page per database.  

The following layout applies to a system page.

| Field Name           | Section            | Data Type | Impl Status | Version Dependent | Description                                                                                                                                                                                     |
| -------------------- | ------------------ | --------- | ----------- | ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PageId               | Page Preamble      | INT       | Yes         | No                | This value is always 0.                                                                                                                                                                         |
| PageType             | Page Preamble      | INT       | Yes         | No                | An enum defined in `PageType.cs`                                                                                                                                                                |
| Database Version     | System Page Layout | INT       | Yes         | No                | A value that denotes the version of this file. As versions change, so do the file formats and how those files are parsed. Different file versions are located in the `Storage\Versions` folder. |
| Database Id          | System Page Layout | GUID      | Yes         | No                | The unique id of the database                                                                                                                                                                   |
| Database Name        | System Page Layout | CHAR(30)  | Yes         | Yes               | The name of the database.                                                                                                                                                                       |
| Created Date         | System Page Layout | DATETIME  | Yes         | Yes               | When the database was created                                                                                                                                                                   |
| Max System Data Page | System Page Layout | INT       | Yes         | Yes               | The maximum system data page id                                                                                                                                                                 |
| Data File Type       | System Page Layout | INT       | No          | Yes               | An enum of the database type found in `Storage\Enum\DataFileType.cs`                                                                                                                            |
| Max User Data Page   | System Page Layout | INT       | No          | Yes               | The maximum user data page id                                                                                                                                                                   |
| Max Index Page       | System Page Layout | INT       | No          | Yes               | The maximum index page id                                                                                                                                                                       |
| Max Contract Page    | System Page Layout | INT       | No          | Yes               | The maximum contract page id                                                                                                                                                                    |


### Page Map
The page map is an in-memory construct used as a directory for the rest of the data file on disk. It is not saved back to disk.

An example of the table looks like the following below. It is constructed and updated in memory as the database file is read.

The code for the `PageMap` and the `PageItem` classes are implemented in the `SystemSchemaConstants.cs` file for the appropriate database version. 

A page map constains `PageItem`s.

#### Example Page Map 

| Order | Page Id | Type        |
| ----- | ------- | ----------- |
| 1     | 1       | System      |
| 2     | 1       | System Data |
| 3     | 2       | System Data |
| 4     | 1       | User Data   |
| 5     | 2       | User Data   |
| 6     | 3       | User Data   |
| 7     | 4       | User Data   |
| 8     | 3       | System Data |
| 9     | 1       | Index       |
| 10    | 5       | User Data   |

In the above example, we can use the Page Map to determine the locations of the user data pages where information may be held.

The page map is constantly updated as changes are made to the data file. 

