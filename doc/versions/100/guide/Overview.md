# Overview
DrummerDB is intended as a proof of concept to try and produce a database system that allows participants in a database to have greater control over information that pertains to them.  

An early collection of these ideas was aggregated into a concept called [Cooperative Database Systems](https://github.com/dynamoRando/CooperativeDatabaseSystems) written by the author.

## Example
In most common Relational Database Systems, data is stored in tables which consist of columns and rows. When laying out a database system, the database author often models the database tables to match the application usage.

As an example, suppose you are building an e-commerce website. A simplified example of a database that might back your website might have the following tables:

- Product
- Product Reviews
- Order
- Customers

The question DrummerDB tries to answer is: who owns the data in the above tables? In most internet applications today, the data being stored by the application is centralized in a database that the users have no rights over. The primary idea of this project is to instead allow portions of the data in the above database to be saved at locations that the users or _participants_ in the database sytem define and control.

For example, the Customers table has information about each customer: probably the Customers Name, Address, and so on. Should that information live in the application's database?

In DrummerDB, the database author (the _host_) could instead allow records in the Customers table to be stored in locations that are tenants of the _host_ DrummerDB that the users control. These users are _participants_ in the database. On the host side, a reference is created to the location of the participant's information. Participants have the option to do anything with these _parts_ of the database sytem. They can delete these records that pertain to them, prevent access at a later point in time to the host, and so on.

To guide how data works in each database, there is a database _contract_ that explicitly states how data in the entire database will work between the host and the participants. A database contract is basically an ACL (Access Control List) over the data as well as a copy of the entire database schema. Every participant gets a copy of the database contract as well as an updated version each time changes are made to it, or the database schema.

Publishing the entire database schema to each participant is also how DrummerDB enables _data portability_. By being able to see how the data in the entire system relates to each other, it allows other applications the ability to read information that may have not originally been intended for it. This allows for useful cases where, for example, a post on a social network site with an understood schema can be "portable" and re-published on other social networks than the original one, without the author of the post (the _participant_) having to take any action.

# Goals
## Primary Goals
This project should produce a database system that:
- Is reasonably performant.
- Is reasonably secure.
- Follows [ACID](https://en.wikipedia.org/wiki/ACID) principles of transactional systems.
- Follows most SQL syntax.
- Allows users to exert authority over peices of information in a database that pertain to them, including determining where that information is stored.
- Allows data portability - meaning that there exists a way to "move" data that may have been part of one database system to be easily consumed by another database system and/or software application/API.
- Is relatively familiar to software developers and is easy to use in application development

## Optional Goals
- The database system may allow for a NoSQL structure mode.

# Disclaimers
Once someone has read access to your data, even once, all bets are off on what they've done with that data. To ensure total security over your data, you not only need to have authority or guarantees in the data tier, but also over the entire application tier and anything in between. 

# Technical Notes
## Language
This project is written in C#. C# was chosen for this project for it's familiarity with the author.

## Microsoft SQL Server
Microsoft SQL Server served as a reference point for this project.

# Motivations
This project is the result of about four years worth of rebooted ideas. It is primarily inspired as a response to the revelation in 2018 of data problems in social media.

# Related Projects
The ideas in this project are not unique. There are similiar projects that also are working towards enabling users to have greater control over their data. The following is a list of projects that the author is aware of.

* [SOLID](https://solidproject.org/)
* [Ocean Protocol](https://oceanprotocol.com/)
* [Blockstack](https://blockstack.org/)
* [IndieWeb](https://indieweb.org/)
* [Data Transfer Project](https://datatransferproject.dev/)
* [GUNDB](https://github.com/amark/gun/blob/master/README.md#what-is-gun)
* [Personal Information Management Systems](https://edps.europa.eu/data-protection/our-work/subjects/personal-information-management-system_en)

