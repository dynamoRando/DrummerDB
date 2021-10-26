# ICacheManager

The cache manager is responsible for maintaining database objects in memory. It does this by maintaining one or several cache objects, usually in the form of a `TreeAddress` and an `ITreeContainer`.

When objects are not in memory, the CacheManager will talk to `IStorageManager` to pull the needed objects from disk into memory, and when needed sync changed objects back to disk via `IStorageManager`.


## Manager Dependencies

IDbManager has dependencies on the following managers -  

- `IStorageManager`
    - This is for pulling database objects from disk if the requested object is not already in memory. In addition, if objects are changed, the cache will attempt to sync the object back to disk.

- `IAuthenticationManager`
    - This is for outbound communications, specifically when there is a request for a remote row to a participant. The cache manager will send the request to the Authentication Manager to stamp the outbound request to `INetworkManager` with the appropriate credentials.
 
## Services Provided

The CacheManager holds one or several cache objects, which are `ConcurrentDictionary` types consisting usually of an identifier and a container or object. 

The identifier of a cache usually is a `GUID` or an _Address_, usually a `TreeAddress`. A `TreeAddress` is nothing more than a way to identify a database and a table: via the Database's `GUID` and the Table's `INT` id.

The object being held can be a `IPage` from disk or a `ITreeContainer`, which is an object that holds a `TreeDictionary` of Pages and the address of the Tree. It's main purpose in holding the object in a container is to provide access to the data in a thread safe manner.

Note: The `TreeDictonary` object is a collection object provided from the [C5 library](https://www.nuget.org/packages/C5).

Futher text goes here.
