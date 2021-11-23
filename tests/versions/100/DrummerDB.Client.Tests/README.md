# Drummersoft.DrummerDB.Client.Tests

## Overview
This is for tests on the Drummersoft.DrummerDB.Client library, which is analogous to the System.Data.SqlClient library. 

It tests the client framework of actions performed against a DrummerDB process.

## General Test Setup

Generally, most tests in this suite need to:

### For a Process
- instantiate a DrummerDB.Process object in a temp directory to prevent database file naming collisions
- delete all files in that temp directory if it already exists to prevent inaccurate test results
- configure an admin login/password for the DrummerDB.Process
- configure that DrummerDB.Process's SQL url and port number

### For a Client
- instatiate a DrummerSQLClient object with the DrummerDB.Process SQL Service url and port number

## TestHarness.cs

To ease some of the setup, there is a TestHarness.cs class that is used in some tests. This class abstracts away some of the boilerplate code
for most tests and assumes that for each test there will be a default database and table that will be used. In general, the usual functions
used by test harness are:

- SetTestObjectNames(): used to configure the default database, table, temp folder name, and SQL port number
- SetupTempDirectory(): ensures that the temp folder for the test exists and if it does, deletes any previous files in it (including databases)
- SetupProcess(): Insantiates the internal DrummerDB.Process object, calls the Start() function of it, and configures the default admin login and session id
- StartNetwork(): Brings online the DrummerDB.Process' SQL service 
- SetupClient(): Instantiates a DrummerDB.Client and configures it with the DrummerDB.Process' SQL port number and URL (default localhost)
- ExecuteSQL(): Executes the specified SQL Statement and returns a SQLQueryReply object. Used to abstract some of the boilerplate code for specifying username/password/sessionId.

## PerfJournal

TestHarness.cs leverages [PerfJournal](https://github.com/dynamoRando/PerfJournal) for recording tests times if wanted. It expects a testSettings.json file in the 
testing directory. To load those settings, you may call LoadJournalSettings() to configure your PerfJournal client.

Functions:
- ConfigureJournalForProjectAsync(): Calls PerfJournal to set it's internal ProjectId for the specified project name. If PerfJournal does not have the 
specified ProjectName, it will ask PerfJournal to create it.
- ConfigureJournalForTestAsync(): Calls PerfJournal to identify the specified TestName's Id. If PerfJournal does not have the specified TestName,
it will ask PerfJournal to create it and return it.
- SaveResultToJournal(): Calls PerfJournal to save a time measurement for a test, indicated by the supplied test id.

