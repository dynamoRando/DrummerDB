namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// Defines the way data will be stored in a table in a database
    /// that has Participants
    /// </summary>
    internal enum LogicalStoragePolicy
    {
        // default - this is when there are no participants in a database
        None,
        // data is only kept at the host
        HostOnly,
        // data is kept at the participant. hashes of the data are kept at the host.
        // if the particpant changes the data, the hash will no longer match
        ParticipantOwned,
        // data is at the host, and changes are automatically pushed to the 
        // participant. if data is deleted at the host, it is not automatically 
        // deleted at the participant but rather a record marker showing it's been
        // deleted is sent to the participant, which the participant can act on
        // or ignore (the marker will still persist)
        // i.e. this is a 'soft' delete to the participant
        Shared,
        // this is basically SQL replication - whatever changes occur at the host will
        // automatically be replicated at the participant. deletes are 'hard'
        // delete actions
        Mirror
    }
}
