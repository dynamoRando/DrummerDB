# Project Breakdown
This project is broken down into four main namespaces, with the Core namespace subdivided among many assemblies.

Note: this page is out of date and needs to be updated.

# Developer Notes
Any object that might be used across multiple assemblies ("XAssembly" for cross assembly) generally exposes an interface. The alternate to this if multiple classes in an assembly share functionality that may want to be enforced.

# Console
This project is meant to be a harness for the Core libary. In the future there may also be a "Service" project that allows the system to be run as a Windows Service or Linux Daemon.

# Client
This project is meant to be a client that talks to the database system from an application. It is intended to be analogus to the "System.Data.SqlClient" namespace in .NET.

# Common
This project contains objects that are common to the Client project and the Core project.

# Core
This is the root of the database system. From this library the database system is further sub-divided into smaller sub-systems. The entry point for each sub-system is suffixed as a "Manager" - so for manging disk access there is a "StorageManager", for memory there is a "CacheManager", and so on. 

The overall subsystems are below.

## Communication
Responsible for all things related to talking on the network.

## Identity & Access
Responsible for all things related to security and authentication.

## Memory
Responsible for memory objects.

## Query
Responsible for handling SQL queries.

## Storage
Responsible for all things related to disk access.

## Structures
Contains most common structures used in the database system. Tables, databases, and so on.

# Core Dependency Chart
In the core libary, the main integration between each managers is roughly as follows.

## Process
A Process object is a singleton. A process is responsible for instantiating all the main "Manager" systems in the database system as singletons - 
- DbManager
    - Manages SQL relational databases objects
- CacheManager
    - Maintains database page objects in memory and is responsible for concurrency management of those objects
- StorageManager
    - Responsible for managing access to database files on disk
- NetworkManager
    - Manages communication between a Client and a Process or another Process
- QueryManager
    - Handles interpretation and execution of SQL formatted queries

## DbManager
A DbManager contains
- StorageManager: to get database metadata
- CacheManager: to pass to each instance of a database

## CacheManager
A CacheManager contains
- StorageManager: to get information from disk
- NetworkManager: to manage remote data changes (talking to Participants)

## StorageManager
A storage manager contains
- CacheManager: only to pass as a reference to each instance of a database

### Internal
- DbFiles: to abstract all the actions that need to happen when writing data back to disk
    - DataFile
    - LogFile

## QueryManager
A query manager contains
- DbManager: to validate passed in queries and to execute them

### Internal
- QueryPlanGenerator: to generate a query plan
- QueryExecutor: to execute a supplied query plan
- QueryParser: to validate a supplied query is valid (syntax and objects exist)
- NetworkManager: to validate assumptions about Participants

## NetworkManager
A network manager contains
- QueryManager: to validate SQL queries that come in from Client and to execute them

# Dependency Principles
Database objects should always talk to Memory, and Memory talks to Storage (or Network if needing to talk to a Participant) if objects are not in memory.

Database objects in memory only contain database metadata; the schemas and so on. The actual data (tables and rows) are held in memory (cache) and/or on disk.

Most of everything happens in a Process happens via a SQL query, which is managed by the Query Manager. These are recieved on the Network (the Client library), which the NetworkManager services TCP messages either from a Client or another Core Process on the network. 

# Insert Row Example
## Statement
INSERT INTO TABLE (ColA, ColB) VALUES ('A', 'B')

## Flow
A **Client** sends the above statement to a **Process** via **Network**. **Network** has a reference to **QueryManager** to validate and generate a query plan to execute.

### Query
**QueryManager** validates the SQL statement:
- Is the syntax valid?
- Do the objects exist in the statement?
- Are the data types correct?

If the above passes, the QueryManager then generates a Query Plan. That paln is sent to the QueryExecutor.

The **QueryExecutor** walks thru each step of the plan and talks to **DbManager** to get the objects and takes actions against them (tables, rows, etc.)

Each object being returned from **DbManager** doesn't actually have data, but instead are talking to **CacheManager** which performs the operations requested. 

**CacheManager** in turn is either talking to **StorageManager** to get things on disk that are not already in memory, or talking to **NetworkManager** if the query involves talking to Participants elsewhere on the network.



