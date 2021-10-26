# IDbManager

This document describes everything under the `IDbManager` system. This system is responsible for modeling SQL databases created by the user (i.e. _user databases_) or databases created by the system itself (i.e. _system databases_.)

For a visualization on how this hiearchy works, see the the file `Db_System.drawio` diagram under the `IDbManager` tab.

## Services Provided

DbManager responds to requests for database information or for storing or providing system level information (such as logins for the database system). It is normally in service of -  
- `IQueryManager` to act on for queries
- `IAuthenticationManager` in the cause of system level authorization for logins, or to handle inbound requests from another database Process on the network (i.e. a remote row call).

It's main responsibility is to hold in memory both a collection of _user databases_ and _system databases_ and provide access to those objects when requested by the above managers.

## Manager Dependencies

IDbManager has dependencies on the following managers -  

- `ICacheManager`
    - This is for passing a reference to the cache to each database instance. 
    
        Remember, databases themselves are an abstraction over objects in memory (_Trees_, which are are just *containers* of _Pages_ with an _Address_) which in turn are objects in memory are just structures loaded from disk. For more information, see `Architecture.md`.
- `ICryptManager`
    - This is for passing a reference to each _system database_ for use in settting up and configuring login information; specifically for use in hashing user passwords.


## Sub Objects

The following are objects that are below `IDbManager` in their dependency hierarchy.

### User Database (Abstract)

This object represents a database object created by a user of the database system.

### HostDb : UserDatabase

This object represents a database created by a user and is a _host_ to participants of a database. For more information on this concept, see `Overview.md`.

#### Dependencies

A Host Db contains the following sub-objects --

- `IDatabaseMetadata` - an object representing an abstraction over the meta data objects in a database. The meta data in a database are the _System Pages_ and _System Data Pages_. 

    For more information, see `Page.md`. These pages are used in maintaining users of the database, object references (such as table and schema information created by the user) and so on.
- `IDatabaseUserdata` - an object representing the user created objects in the database, such as tables, views, stored procedures, and so on.

For more information on these sub-objects, see further in this document.

### IDatabaseMetadata

This object is normally hosted inside of a User Database (Abstract) and serves as an abstraction over system pages and system data pages. It abstracts them by treating
them as actual tables, instead of having to interface with them as Trees of Pages (as `ICacheManager` sees them).

It has references to the following objects -

#### References

- `ICacheManager`: For I/O to System Pages and System Data Pages
- `ICryptoManager`: For communicating with cache manager on user information (authentication, etc).
- `IDbManager`: For validating user information in the event the user accessing the database is a system level administrator.

Further text goes here.