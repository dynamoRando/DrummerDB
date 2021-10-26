# INetworkManager

The network manager is responsible for communications for the database system. The networking system is built on top of [gRPC for C#](https://docs.microsoft.com/en-us/aspnet/core/grpc/basics).

It exposes three gRPC services for interating with a Process.

* SQL Service
    * This service handles SQL queries/statements that should be processed by the database system. It is mainly intended to service requests from users of the Drummersoft.DrummerDB.Client library.
* Database Service
    * This service allows interaction directly with database objects. It is mainly intended to service requests from other database systems (usually for partial database [participant] communications) but can be also called from the Drummersoft.DrummerDB.Client library.
* Info Service
    * This service is intended to return various information about the database system. It returns schema information, database status, number of databases, and so on.

# Service Consumption
For most service calls, an `AuthRequest` (Authentication Request) message must be sent along with the actual method parameters. 

An AuthRequest object consists of the following parameters:

## AuthRequest Format

| Field Name | Data Type | Order | Description                                                                                    |
| ---------- | --------- | ----- | ---------------------------------------------------------------------------------------------- |
| UserName   | String    | 1     | A user of the datbabase system                                                                 |
| PW         | String    | 2     | The user's password                                                                            |
| PWHash     | Bytes     | 3     | A hash of the user's password                                                                  |
| Token      | String    | 4     | An alt of authentication of un/pw. You can instead send an auth token to identify the request. |
|            |           |       |                                                                                                |

## AuthResult Format
For most service calls, an `AuthResult` object will be returned identifying if the authorization sent with the request was successful.

| Field Name            | Data Type | Order | Description                                             |
| --------------------- | --------- | ----- | ------------------------------------------------------- |
| UserName              | String    | 1     | A user of the datbabase system                          |
| Token                 | String    | 2     | An echo back of the token sent                          |
| IsAuthenticated       | Bool      | 3     | True/False if the authentication attempt was successful |
| AuthenticationMessage | String    | 4     | Any additional information about the auth attempt       |
|                       |           |       |                                                         |

In addition, all services provide a test method named `IsOnline` to see if the service is responding.

For the three services (SQL, Database, Info), there are common shared objects that are used between them which live in the `Drum.proto` file. The `AuthRequest` and the `AuthResult` objects, for example, are common to all three services.

## Is Online Format

The `IsOnline` method usually echo's back a sent message to the service. It takes a `TestRequest` object and responds with a `TestReply` object.

### Test Request Format

| Field Name           | Data Type | Order | Description                                                         |
| -------------------- | --------- | ----- | ------------------------------------------------------------------- |
| Request Time UTC     | String    | 1     | The time the request was sent in UTC                                |
| Request Origin URL   | String    | 2     | The Origin URL of the request                                       |
| Request Origin IP 4  | String    | 3     | The Origin IP Address in IP4 format                                 |
| Request Origin IP 6  | String    | 4     | The Origin IP Adress in IP6 format                                  |
| Request Port Number  | Int32     | 5     | The request port number                                             |
| Request Echo Message | String    | 6     | A request message that will be echo'd back if the service is online |
|                      |           |       |                                                                     |

### Test Reply Format

| Field Name         | Data Type | Order | Description                                                   |
| ------------------ | --------- | ----- | ------------------------------------------------------------- |
| Reply Time UTC     | String    | 1     | The time the reply was generated in UTC                       |
| Reply Echo Message | String    | 2     | A response message that was sent in the requset (echo'd back) |
|                    |           |       |                                                               |

## SQL Service

### Various Method Calls Go Here

Notes go here.

## Database Service

The database service allows direct interaction with various system level objects, such as database tables, etc. This service can handle requests from `Client`s or other database `Process`es on the network.

| Method Name        | Input Message         | Response Message     | Description                                       |
| ------------------ | --------------------- | -------------------- | ------------------------------------------------- |
| IsOnline           | TestRequest           | TestReply            | Basic test to see if the service is responding    |
| IsLoginValid       | AuthRequest           | AuthResult           | Validates that the specified login is active      |
| CreateUserDatabase | CreateDatabaseRequest | CreateDatabaseResult | Creates a database with the specified information |
| RemoveUserDatabase | RemoveDatbaseRequest  | RemoveDatabaseResult | Removes the specified user database               |
|                    |                       |                      |                                                   |

### Methods
The Database Service exposes the following methods

#### CreateUserDatabase

This method creates a user database based on the supplied information in the `CreateDatabaseRequest` message and returns a `CreateDatabaseResult`.

##### CreateDatabaseRequest

| Field Name     | Data Type   | Order | Description                                |
| -------------- | ----------- | ----- | ------------------------------------------ |
| Authentication | AuthRequest | 1     | The user attempting to create the database |
| DatabaseName   | String      | 2     | The name of the database to be created     |
|                |             |       |                                            |

#### CreateDatabaseResult

| Field Name           | Data Type  | Order | Description                                            |
| -------------------- | ---------- | ----- | ------------------------------------------------------ |
| AuthenticationResult | AuthResult | 1     | The result of the user authentication attempt          |
| IsSuccessful         | Bool       | 2     | The result of the database creation attempt            |
| DatabaseName         | String     | 3     | An echo response of the database name that was created |
| ResultMessage        | String     | 4     | Details of the create database attempt                 |
| DatabaseId           | String     | 5     | The unique GUID of the database created                |
|                      |            |       |                                                        |

##### RemoveDatabaseRequest

| Field Name     | Data Type   | Order | Description                                |
| -------------- | ----------- | ----- | ------------------------------------------ |
| Authentication | AuthRequest | 1     | The user attempting to remove the database |
| DatabaseName   | String      | 2     | The name of the database to be remove      |
|                |             |       |                                            |

#### RemoveDatabaseResult

| Field Name           | Data Type  | Order | Description                                            |
| -------------------- | ---------- | ----- | ------------------------------------------------------ |
| AuthenticationResult | AuthResult | 1     | The result of the user authentication attempt          |
| IsSuccessful         | Bool       | 2     | The result of the database removal attempt             |
| DatabaseName         | String     | 3     | An echo response of the database name that was removed |
| ResultMessage        | String     | 4     | Details of the database removal attempt                |
| DatabaseId           | String     | 5     | The unique GUID of the database removed                |
|                      |            |       |                                                        |



## Info Service

### Various Method Calls Go Here

 Notes Go Here