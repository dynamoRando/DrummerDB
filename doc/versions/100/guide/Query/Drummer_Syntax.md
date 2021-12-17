# DrummerDB Syntax Guide
## Overview
This document lists out various "Drummer" commands that are unique to the DrummerDB database and are not SQL standard.

A "Drummer" command follows the syntax:

```
DRUMMER BEGIN;
{command};
DRUMMER END;
```

As an example:
```
DRUMMER BEGIN;
SET LOGICAL STORAGE FOR dbo.Customer Participant_Owned;
DRUMMER END;
```

Internally, Drummer commands are handled seperately from regular SQL commands, as SQL commands are handled by [Antr](https://www.antlr.org/) generated code provided by [this](https://github.com/antlr/grammars-v4/tree/master/sql/tsql) grammar.

> Note:
In the hopeful future, it would be nice to have all grammars handled by one Antlr generated grammar that understands DrummerDB syntax as well as T-SQL syntax.

## Set Logical Storage
### Syntax
`SET LOGICAL STORAGE FOR {tableName} [policy]`

### Description
Sets the logical storage policy for a specific table. For a database host to participate with others, a policy must be set for each table.
The database policy options are:

*	None
    * No policy has been set.
*	Host_Only
    * Data is only stored at the host and the host claims full authority of the data in the table.
*	Participant_Owned
    * Data is only stored at the participant for participant identified rows. A row id reference and data hash is kept at the host. Data is not saved to the transaction log at the host, but the row id and the hash is.
*   Shared
    * Data is stored the host and changes are replicated to the participant. Deletes at the host are “soft” deletes for the participant, meaning the record is marked as deleted, but not actually deleted unless the participant wishes to refactor their local copy of the database.
*	Mirror
    * Data is replicated from the host and saved at the participant. Neither claims full authority at either end. Deletes at one location do not affect the other but are “hard” meaning the data is permanently deleted at either end.

## Review Logical Storage
### Syntax
`REVIEW LOGICAL STORAGE FOR {tableName}`

### Description
Returns the logical storage policy for the specified table. This is interpreted to be a SELECT statement by the query processor: 

`SELECT LogicalStoragePolicy FROM sys.UserTables WHERE TableName = {tableName}`

## Generate Host Info As
### Syntax
`GENERATE HOST INFO AS HOSTNAME {alias}`

