# DrummerDB

An attempt at writing a proof-of-concept relational-style database system in C#. 

Also an implementation of a [Cooperative Database System](https://github.com/dynamoRando/CooperativeDatabaseSystems) - an idea where rows of the tables in a database can be stored at various locations where users can exert control. 

Based on a revised version of the [FrostDb](https://github.com/dynamoRando/FrostDB) codebase, which is no longer active.

This project is being actively developed, and does not yet have a release.

## Project Goals

This project hopes to enable a system where users have greater control over their data. This project also hopes to help developers building multi-tenant systems.

## Major Concepts

### Authority Over Data
For users to have authority over their data, they must have the ability to do the following things:

* Define where their data is stored.
* Define who has access to their data, and the ability to change it at any time.
* Consume their data in any software application of their choosing; regardless of the original application that they put their data into. This is data _interoperability_ or portability.

### Implementation
DrummerDB tries to account for the first two items by allowing rows in tables to be references to other instances of DrummerDB.  These reference rows point to other DrummerDB instances that have _partial_ databases and partial tables that have the exact same schema as the _host_ database system; but contain the actual tuples of data. In turn, partial databases have references back to the host database system.

When a database author creates a database in DrummerDB, they define the schema just as they would in any regular RMDBS. To enable users (called _participants_) to have "their" data, database authors (called _hosts_) define data contracts that are sent to participants who either host their own instance of DrummerDB or leverage a provider that they trust to manage a DrummerDB instance on their behalf.

A data contract includes the schema of the _entire_ database system and includes initial logical storage policies where the data will be stored: in tables of the _host_, or in tables of the _participants_, or a duplicate copy in both locations.

Participants accept (or reject) data contracts and can override the initial settings of tables that are created in their own hosted instance of DrummerDB.

Data that ultimately belongs to participants are sent to participants via DrummerDB specific keywords that are an extention of SQL.

Schema changes to the database automatically generate new data contracts that must be accepted by all existing participants of a database system.

DrummerDB tries to allow for the data _interoperability_ by making the schemas to be public, and hopefully later allowing metadata to be added to the schema of a database (references to Linked Data, RDF, etc.)

## Remarks

I don't consider myself a C# developer. I am learning as I go along.

# License

This project is licensed under the [Mozilla Public License v2](https://www.mozilla.org/en-US/MPL/2.0/). 

This project leverages the following other projects / libraries --

| Project / Library | License                           | Website                                         | Usage                                                                                                                |
| ----------------- | --------------------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| Antlr             | BSD-3                             | [Antlr.org](https://www.antlr.org/license.html) | SQL parsing based on [T-SQL](https://github.com/antlr/grammars-v4/tree/master/sql/tsql) |
| C5                | MIT                               | [C5 Github](https://github.com/sestoft/C5/)     | Leverages a tree dictionary for holding pages in memory                                                                                                                     |
| gRPC              | Creative Commons 2.0 / Apache 2.0 | [grpc.io](https://grpc.io/)                     | Used for various communication purposes                                                                                                                     |

