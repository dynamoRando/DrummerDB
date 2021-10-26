using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /*
     * brainstorming ideas. thinking that this would be a parameter to anything at the database or lower level. The mode would determine
     * if the action is saved to the log file or the data file, or removed from the log file (if rollback).
     */
    internal enum TransactionMode
    {
        /// <summary>
        /// Default for unknown status or prototyping. 
        /// </summary>
        Unknown,
        /// <summary>
        /// Attempt the operation and save the action data to the log file with a flag of uncommited for the transaction id. This modifies the 
        /// object in memory, but not on disk.
        /// </summary>
        Try,
        /// <summary>
        /// Delete the transaction data from the log file for the specified transaction. This reverts the object in memory, and deletes all records
        /// from the log file.
        /// </summary>
        Rollback,
        /// <summary>
        /// Updates the log file for the specified transaction with IsCompleted flag set. To be determined if this also means that the record
        /// should be persisted to the data file immeidately, or if there should be a pending data file queue (CHECKPOINT) to be done 
        /// at a later time.
        /// </summary>
        Commit,
        /// <summary>
        /// Don't perform the action at all with any transactional capabilities
        /// </summary>
        None
    }
}
