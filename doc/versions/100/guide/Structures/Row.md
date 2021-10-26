# Data Row Layout
Rows are saved in binary format on a page, sorted by _binary order_. This means that fixed binary length columns such as INT and BOOL are saved first, according to the column's ordinal value, and then variable length columns, such as VARCHAR(10), are saved after.  

For variable length columns, the size of the column's data is prefixed as an INT value before the actual data. For fields that are marked as NULLABLE, there is a 1 byte BOOL prefix for the column indicating if the value is NULL or not. If it is, the field is compressed and the value is not supplied.

Each row contains a _preamble_ which is a fixed size section that contains metadata about the row.

## Row Layout
The following table describes the layout of a row.

| Field Name                 | Section  | Data Type   | Description                                                                                                                                                                                                                                                                                                                                   |
| -------------------------- | -------- | ----------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| RowId                      | Preamble | INT         | A unique id for the row. There should only be 1 row id per row. However, rows that have been updated can be _forwarded_, meaning that there may be multiple row ids on a page. For more information, see the section _Row Forwarding_.                                                                                                        |
| IsLocal                    | Preamble | BOOL        | If the row is local to the database host or not.                                                                                                                                                                                                                                                                                              |
| IsDeleted                  | Preamble | BOOL        | If the row is deleted or not. Row data is not actually removed from disk until the table is rebuilt.                                                                                                                                                                                                                                          |
| IsForwarded                | Preamble | BOOL        | If the row has been updated and the updated values are not able to fit in the existing position of the row on the page, a row may be _forwarded_ to another location on the page or on to another page. This flag denotes this.                                                                                                               |
| ForwardOffset              | Preamble | INT         | The number of bytes from the start of the page to read if the row was forwarded. This defaults to 0 if the row is not forwarded.                                                                                                                                                                                                              |
| ForwardedPageId            | Preamble | INT         | If the row was updated (forwarded) but not able to fit on the current page, this value denotes the Page where the row can be found. This value may be set to the current PageId if the row is forwarded to the same page, or to another page if it's update did not fit on the page.                                                          |
| SizeOfRow or ParticipantId | Row Data | INT or GUID | If `IsLocal == TRUE`, this value is the rest of the size of the row in bytes minus the preamble. It includes the INT value of the field itself (in other words, it includes the SizeOfRow size, which is an INT).  If `IsLocal == FALSE`, this value is a GUID which is the ParticipantId where the rest of the row data is actually located. |
| Row Data                   | Row Data | Variable    | The rest of the data is the row itself, sorted in binary order. For more information, see the next section.                                                                                                                                                                                                                                   |

## Data Example
Suppose that you had a table named _Employee_ with the following schema:  

| Column Name  | Data Type   | Ordinal | Binary Size                                                           |
| ------------ | ----------- | ------- | --------------------------------------------------------------------- |
| Id           | INT         | 1       | 4                                                                     |
| Age          | INT         | 2       | 4                                                                     |
| Name         | VARCHAR(10) | 3       | ? - this can be any value up to 10 in length, since it is _variable._ |
| IsTerminated | BOOL        | 4       | 1                                                                     |

In binary order this would be laid out as:

| Column Name  | Data Type   | Order |
| ------------ | ----------- | ----- |
| Id           | INT         | 1     |
| Age          | INT         | 2     |
| IsTerminated | BOOL        | 3     |
| Name         | VARCHAR(10) | 4     |

Suppose that you had the following values to insert into the table, with the last value being a **remote remote insert**:

| Id  | Age | Name      | IsTerminated |
| --- | --- | --------- | ------------ |
| 1   | 24  | Jim       | FALSE        |
| 2   | 34  | Cornelius | FALSE        |
| 3   | 33  | Jessica   | TRUE         |

Size-wize, these rows would be converted as:

| Row Id | Size (Bytes) | Formula                                                                                                                                                                                       |
| ------ | ------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1      | 16           | Id: INT (4) + Age: INT (4) + Name: Size Of Variable Field Prefix as INT (4), which would hold the numeric value 3 + VARCHAR field data(3) which would be 'Jim' + IsTerminated: BOOL (1)       |
| 2      | 22           | Id: INT (4) + Age: INT (4) + Name: Size Of Variable Field Prefix as INT (4), which would hold the numeric value 9 + VARCHAR field data(9) which would be 'Cornelius' + IsTerminated: BOOL (1) |
| 3      | 16           | Size Of Participant Id, which is a GUID and 16 bytes                                                                                                                                          |

Conceptually, the rows would be laid out on the page as follows (including the preambles). Note again that the row values are sorted in binary order, meaning fixed length fields come first. Also note that SizeOfRow is 4 bytes bigger than the actual size of the row, making allowance for the size of the value itself (which is an INT).

| Field           | Value                                                  | Type and Size |
| --------------- | ------------------------------------------------------ | ------------- |
| RowId           | 1                                                      | INT - 4       |
| IsLocal         | TRUE                                                   | BOOL - 1      |
| IsDeleted       | FALSE                                                  | BOOL - 1      |
| IsForwarded     | FALSE                                                  | BOOL - 1      |
| ForwardOffset   | 0                                                      | INT - 4       |
| ForwardedPageId | 1                                                      | INT - 4       |
| SizeOfRow       | 20                                                     | INT - 4       |
| Id              | 1                                                      | INT - 4       |
| Age             | 24                                                     | INT - 4       |
| IsTerminated    | FALSE                                                  | BOOL - 1      |
| SizeOfName      | 3                                                      | INT - 4       |
| Name            | Jim                                                    | CHAR - 3      |
| RowId           | 2                                                      | INT - 4       |
| IsLocal         | TRUE                                                   | BOOL - 1      |
| IsDeleted       | FALSE                                                  | BOOL - 1      |
| IsForwarded     | FALSE                                                  | BOOL - 1      |
| ForwardOffset   | 0                                                      | INT - 4       |
| ForwardedPageId | 1                                                      | INT - 4       |
| SizeOfRow       | 26                                                     | INT - 4       |
| Id              | 2                                                      | INT - 4       |
| Age             | 34                                                     | INT - 4       |
| IsTerminated    | FALSE                                                  | BOOL - 1      |
| SizeOfName      | 9                                                      | INT - 4       |
| Name            | Cornelius                                              | CHAR - 9      |
| RowId           | 3                                                      | INT - 4       |
| IsLocal         | FALSE                                                  | BOOL - 1      |
| IsDeleted       | FALSE                                                  | BOOL - 1      |
| IsForwarded     | FALSE                                                  | BOOL - 1      |
| ForwardOffset   | 0                                                      | INT - 4       |
| ForwardedPageId | 1                                                      | INT - 4       |
| ParticipantId   | 66223ff1-b3ee-4971-a2e1-90ea0ddfacbf (an example GUID) | GUID - 16     |

# NULL Values
If a column has been marked as "Nullable", the byte array value will be prefixed with a `BOOL` indicating if the value is NULL or not. If the value is `TRUE`, the actual byte array of the value will be skipped. As an example: suppose a row has the following schema - 

| Field      | Data Type   | Is Nullable | Byte Layout                                           |
| ---------- | ----------- | ----------- | ----------------------------------------------------- |
| Id         | INT         | FALSE       | INT - 4                                               |
| Age        | INT         | FALSE       | INT - 4                                               |
| First Name | VARCHAR(10) | FALSE       | INT 4 - (for the length) VARCHAR - 10 max             |
| Nick Name  | VARCHAR(10) | TRUE        | BOOL - 1, INT 4 - (for the length) - VARCHAR - 10 max |
| Height     | INT         | TRUE        | BOOL - 1, INT 4                                       |
|            |             |             |                                                       |

As an example, suppose you wanted to insert the following values into the table:

| Id  | Age | First Name | Nick Name | Height |
| --- | --- | ---------- | --------- | ------ |
| 1   | 30  | Bob        | NULL      | NULL   |
| 2   | 31  | Jessica    | Jess      | 63     |
| 3   | 34  | Rachel     | NULL      | 54     |
|     |     |            |           |        |

The corresponding byte layout would look like the following:

| Row Id | Size (Bytes) | Formula                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------ | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1      | 17           | Id: INT (4) + Age: INT (4) + First Name: Size Of Variable Field Prefix as INT (4), which would hold the numeric value 3 + VARCHAR field data(3) which would be 'Bob' + Nick Name: BOOL (1), which would be `TRUE` because the value is NULL + Height: BOOL (1) which would be `TRUE` because the value is NULL                                                                                                                                                     |
| 2      | 32           | Id: INT (4) + Age: INT (4) + First Name: Size Of Variable Field Prefix as INT (4), which would hold the numeric value 7 + VARCHAR field data(7) which would be 'Jessica' + Nick Name: BOOL (1), which would be `FALSE` because the value is NOT NULL + INT (4), which would hold the numeric value 4 + VARCHAR field data (3) which would be 'Jess' + Height: BOOL (1) which would be `FALSE` because the value is NOT NULL + INT(4) which would hold the value 63 |
| 3      | 24           | Id: INT (4) + Age: INT (4) + First Name: Size Of Variable Field Prefix as INT (4), which would hold the numeric value 6 + VARCHAR field data(6) which would be 'Rachel' + Nick Name: BOOL (1), which would be `TRUE` because the value is NULL + Height: BOOL (1) which would be `FALSE` because the value is NOT NULL + INT(4) which would hold the value 54                                                                                                      |



## NULL and RowValue

As already mentioned, in a Page if a column is marked as `IsNullable`, this means that at a minimum the field is 1 byte (the size of a `BOOL`.)

### Fixed Sized Values (INT, BOOL, etc.)

If a field is a fixed with size and is also `Nullable`, the field is stored in the RowValue's `_value` byte[] field with the boolean value (IsNull) + the regular fixed size if it is not NULL. This is also how it should be stored on the page. 

If the field is NULL, the field is only 1 byte (size of BOOL). This is how it is stored both in RowValue's `_value` field as well as on the Page.

As an example, if a column was marked `Nullable` and wanted to save a Not Null value of 10 for a column named "Rank", the `_value`  byte[] would look like the following:

| Item   | Size     | Value |
| ------ | -------- | ----- |
| IsNull | BOOL - 1 | False |
| Value  | INT - 4  | 10    |
|        |          |       |

This is both how it should be saved onto a Page, how it is held in the RowValue's internal field `_value`, and how it should be passed into `RowValue` from reading data from the Page (5 bytes wide, the initial NOT NULL and the actual value).

### Variable Sized Values (Varchar, Char, Varbinary, Binary)

If the field is a variable size, the RowValue's `_value` byte field holds the boolean value and the actual character value.

When the value is retreved to be saved onto the Page, the array is prefixed with the variable's size. In other words, while the byte array for a variable RowValue item looks like this (take as an example a Column Named "Name" which has a value of "Jennifer") -

| Item   | Size     | Value    |
| ------ | -------- | -------- |
| IsNull | BOOL - 1 | False    |
| Value  | CHAR(8)  | Jennifer |
|        |          |          |

When the value is retrieved to be saved onto the page, the array returned looks like this:

| Item                   | Size     | Value    |
| ---------------------- | -------- | -------- |
| IsNull                 | BOOL - 1 | False    |
| Size Of Variable Field | INT - 4  | 8        |
| Value                  | CHAR(8)  | Jennifer |
|                        |          |          |

However, when the value is saved/set into a RowValue object, it does not retain the length, but again looks like the following layout - 

| Item   | Size     | Value    |
| ------ | -------- | -------- |
| IsNull | BOOL - 1 | False    |
| Value  | CHAR(8)  | Jennifer |
|        |          |          |

This is because the length of the field is only needed for determining how many bytes to read from a Page, but is not needed for actual operations on a `RowValue`.

