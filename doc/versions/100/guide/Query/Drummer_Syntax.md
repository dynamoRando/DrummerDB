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

> Note:
This document is updated as commands are expanded upon. It does not yet fully go into each system internals for each command.

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

### Request Host Notify Accepted Contract
### Syntax
`REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {hostAlias}`

### Description
Generates a network request to notify the host that the participant has accepted the latest contract with the specified alias.

