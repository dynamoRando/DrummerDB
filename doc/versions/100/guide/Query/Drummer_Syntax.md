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

The command _must be wrapped_ by the keywords `DRUMMER BEGIN` and `DRUMMER END`.

Internally, Drummer commands are handled seperately from regular SQL commands, as SQL commands are handled by [Antr](https://www.antlr.org/) generated code provided by [this](https://github.com/antlr/grammars-v4/tree/master/sql/tsql) grammar.

> Note:
In the hopeful future, it would be nice to have all grammars handled by one Antlr generated grammar that understands DrummerDB syntax as well as T-SQL syntax.

> Note:
This document is updated as commands are expanded upon. It does not yet fully go into each system internals for each command.

For any code example, items wrapped in curly braces `{example}` are values that are to be specified by the author, for example a database name or a table name. 

For items wrapped in square braces `[example]` these are specific expected values, usually described in the command itself (for example, setting an option `ON` or `OFF`) or described in a table in the same section.

For Drummer commands, specify the end of the line by closing it with a semicolon.

# Drummer Syntax Commands

## Set Logical Storage
### Syntax
`SET LOGICAL STORAGE FOR {tableName} [policy]`

### Description
Sets the logical storage policy for a specific table. For a database host to participate with others, a policy must be set for each table.
The logical storage policy options are:

| Policy            | Description                                                                                                                                                                                                                                                                      |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| None              | No policy has been set.                                                                                                                                                                                                                                                          |
| Host_Only         | Data is only stored at the host and the host claims full authority of the data in the table.                                                                                                                                                                                     |
| Participant_Owned | Data is only stored at the participant for participant identified rows. A row id reference and data hash is kept at the host. Data is not saved to the transaction log at the host, but the row id and the hash is.                                                              |
| Shared            | Data is stored at the host and changes are replicated to the participant. Deletes at the host are “soft” deletes for the participant, meaning the record is marked as deleted, but not actually deleted unless the participant wishes to refactor their local copy of the database. |
| Mirror            | Data is replicated from the host and saved at the participant. Neither claims full authority at either end. Deletes at one location do not affect the other but are “hard” meaning the data is permanently deleted at either end.                                                |
|                   |                                                                                                                                                                                                                                                                                  |

## Review Logical Storage
### Syntax
`REVIEW LOGICAL STORAGE FOR {tableName}`

### Description
Returns the logical storage policy for the specified table. This is interpreted to be a SELECT statement by the query processor: 

`SELECT LogicalStoragePolicy FROM sys.UserTables WHERE TableName = {tableName}`

## Set Notify Host
### Syntax
`SET NOTIFY HOST FOR {partialDatabaseName} TABLE {tableName} OPTION [on|off]`

### Description
Configures the specified table in the specified partial database to notify it's remote counterpart (the host) of any data changes made to the table. By default when a contract is first generated this value is set to `TRUE`.

### Context
A Participant has full authority over their tables in the partial database. As data is inserted from the host and saved at the participant, the participant also has the option to update their data locally. 

Whenever a row is saved at a participant, a data hash of the values is saved at the host. 

Should the participant update the values locally in their database, this option determines if the database system will attempt to update the host
of the changed data hash if the row is updated. 

If the row is deleted locally at the participant, this option will determine if the database system will update the host to notify that the row was deleted remotely (from the host's perspective).

## Generate Host Info As
### Syntax
`GENERATE HOST INFO AS HOSTNAME {alias}`

### Description
Generates a record in the "coop.HostInfo" system table in the system database with the specified host alias.

The HostInfo table contains the following schema:

| Column Name | Data Type       | Description                                                                                                                               |
| ----------- | --------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| HostGUID    | CHAR (36)       | Generated when the above command is ran.                                                                                                  |
| HostName    | VARCHAR (128)   | Specified with the above command                                                                                                          |
| Token       | VARBINARY (128) | Generated when the above command is ran. This is essentially a security key that is sent to all participants for authentication purposes. |
|             |                 |                                                                                                                                           |

## Generate Contract With Description
### Syntax
`GENERATE CONTRACT WITH DESCRIPTION {message}`

### Description
Generates a database contract with the host name as the author of the contract and the specified description/message.

> Note: for a contract to be generated, a Logical Storage Policy must be set for all user tables in the database, even if the tables are not participating in a cooperative manner.

## Add Participant
### Syntax
`ADD PARTICIPANT {alias} AT {ipAddress:portNumber}`

### Description
Adds a participant to the "sys.Participants" table in the host database with the specified alias and ip 4 address and database port number.

The "sys.Participants" table has the following schema:

| Column Name                     | Data Type       | Description                                                   |
| ------------------------------- | --------------- | ------------------------------------------------------------- |
| ParticipantGUID                 | CHAR (36)       | A unique id for the participant                               |
| Alias                           | VARCHAR (20)    | An alias for the participant, specified in this command       |
| IP4Address                      | VARCHAR (20)    | The IP address for the participant in ip4 format              |
| IP6Address                      | VARCHAR (20)    | The IP Address for the participant in ip6 format              |
| PortNumber                      | INT             | The _database port_ for the participant.                      |
| LastCommunicationUTC            | DATETIME        | The last time any communication was held with the participant |
| Status                          | INT (Enum)      | The current contract status with the participant              |
| Accepted Contract Verison       | CHAR (36)       | The last contract version the participant accepted            |
| Accepted Contract Date Time UTC | DATETIME        | The time the last contract was accepted                       |
| Token                           | VARBINARY (128) | A unique token identifying the participant                    |


## Request Participant Save Contract
### Syntax
`REQUEST PARTICIPANT {alias} SAVE CONTRACT`

### Description
Generates a network request for the participant with the specified alias to save the latest contract for acceptance/rejection.

## Review Pending Contract
### Syntax
`REVIEW PENDING CONTRACTS`

### Description
Returns a list of pending contracts to be accepted/rejected.

## Accept Contract By
### Syntax
`ACCEPT CONTRACT BY {hostAlias}`

### Description
Accepts the latest pending contract fort he specified host.

This action will mark the contract as saved in the "coop" (Cooperative) system tables and then generate a partial database with the contract's specified schema.

## Review Accepted Contracts
### Syntax
`REVIEW ACCEPTED CONTRACTS`

### Description

Returns all contracts that have been accepted.

## Request Host Notify Accepted Contract
### Syntax
`REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {hostAlias}`

### Description
Generates a network request to notify the host that the participant has accepted the latest contract with the specified alias.

## Set Remote Delete Behavior For 
### Syntax
`SET REMOTE DELETE BEHAVIOR FOR {hostDatabaseName} OPTION [option]`

### Description
Sets the remote deletion behavior for a host database based on the option provided. 

### Context
A participant has full authority of the data on their side - including deleting the data from their partial database. When this situation happens, there is a reference row at the host database that now points to a remote data row at the participant that no longer exists.

When the host discovers that the reference to the data the participant has is no longer valid (i.e. the row data has been deleted) the options the host can take are:


| Option             | Description                                                                                                                                                                                                      |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unknown            | Default if no value is set. This is a default programming setting.                                                                                                                                               |
| Ignore             | Take no action.                                                                                                                                                                                                  |
| Auto_Delete        | Updates the local reference row in the host database with the deletion information (updates the row as remotely deleted, the deletion date and time in UTC), and then actually deletes the row on it's side.     |
| Update_Status_Only | Updates the local reference row in the host database with the deletion informaiton (updates the row as remotely deleted, the deletion date and time in UTC), but does not delete the row from the host database. |


