# DrummerDB

An attempt at writing a database system in C#. 

Also an implementation of a [Cooperative Database System](https://github.com/dynamoRando/CooperativeDatabaseSystems) - an idea where rows of the tables in a database can be stored at various locations where users can exert control. 

Based on a revised version of the [FrostDb](https://github.com/dynamoRando/FrostDB) codebase, which is no longer active.

This project is being actively developed, and does not yet have a release.

## Project Goals

This project hopes to enable a system where users have greater control over their data. This project also hopes to help developers building multi-tenant systems.

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

