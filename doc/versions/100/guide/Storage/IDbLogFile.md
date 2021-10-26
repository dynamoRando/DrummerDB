# IDbLogFile

The log file is the database log that holds all the transactions that have occured in the database. 

# File Format
The log file always consists of a two entry format: the size of the entry and the actual entry data itself.

| Item Name  | Data Type | Description                               |
| ---------- | --------- | ----------------------------------------- |
| Entry Size | `INT`     | The number of bytes for the entry         |
| Entry      | Binary    | The actual entry (format in next section) |
 
## Entry File Format

| Field Name               | Implementation Status | Binary Length | Section            | Data Type      | Description                                                                                                                                                                                           |
| ------------------------ | --------------------- | ------------- | ------------------ | -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| IsCompleted              | Yes                   | Fixed         | Preamble           | `Bool`         | If the transaction is completed or not                                                                                                                                                                |
| TransactionId            | Yes                   | Fixed         | Preamble           | `Guid`         | The unique id for the transaction                                                                                                                                                                     |
| TransactionBatchId       | Yes                   | Fixed         | Preamble           | `Guid`         | The unique id that the transaction participates in                                                                                                                                                    |
| AffectedObjectId         | Yes                   | Fixed         | Preamble           | `Guid`         | The database object id that is being impacted                                                                                                                                                         |
| EntryTimeUTC             | Yes                   | Fixed         | Preamble           | `DateTime`     | The time the transaction was started in UTC                                                                                                                                                           |
| CompletedTimeUTC         | Yes                   | Fixed         | Preamble           | `DateTime`     | The time the transction was completed                                                                                                                                                                 |
| TransactionActionType    | Yes                   | Fixed         | Preamble           | `INT` (`Enum`) | The type of action that was taken                                                                                                                                                                     |
| TransactionActionVersion | Yes                   | Fixed         | Preamble           | `INT`          | The version of the transaction format                                                                                                                                                                 |
| Action Binary Length     | Yes                   | Fixed         | Preamble           | `INT`          | The binary length of the action (the next field)                                                                                                                                                      |
| Action                   | Partially             | Variable      | Transaction Action | Variable       | A `byte[]` value that are the binary values of the action. Use the above `TransactionActionType` to figure out what to cast the object to. For more information, see the section "Action File Format" |
| UserNameLength           | No                    | Fixed         | Entry              | `INT`          | The length of the user name field                                                                                                                                                                     |
| UserName                 | No                    | Variable      | Entry              | `string`       | The user who executed this transaction                                                                                                                                                                |
| QueryPlan                | No                    | ?             | ?                  | ?              | ?                                                                                                                                                                                                     |


For more information on the Action data layout, see the next section.

# Action File Format
This section describes each of the types of Transaction Actions and their binary formats.

## Transaction Action Data Format

| Item        | Binary Length | Section | Data Type              | Description                                                                                                                                                        |
| ----------- | ------------- | ------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Operation   | Fixed         | Action  | `INT` (`Enum`)         | The action being tabled (SELECT, INSERT, etc.)                                                                                                                     |
| SQL Address | Fixed         | Action  | `GUID` (1x) `INT` (4x) | The SQL Address of the action being taken on. The first item is a `GUID` (the database id) and the other items are `INT`s describing the location in the database. |
 


