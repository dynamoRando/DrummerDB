﻿using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    // note: contracts are issued per database
    // a host may issue multiple contracts to the same participant if it's for a different database
    // this may mean we need to refactor how this object works
    internal record struct Contract
    {
        public HostInfo Host { get; set; }
        // The GUID of the contract, found in all the schema tables
        // in the HostDb metadata. this is unique to the contract but does not change between schema changes
        public Guid ContractGUID { get; set; }
        // the date the contract was generated
        public DateTime GeneratedDate { get; set; }
        // a description of the contract agreement
        public string Description { get; set; }
        // the name of the database, this should be the same as the 
        // actual name of the host database
        public string DatabaseName { get; set; }
        // the acutal id of the database, this should be the value
        // from the host database
        public Guid DatabaseId { get; set; }
        // All the tables in the database. Note that we need
        // the StoragePolicy property needs to be populated for each
        // table
        public List<ITableSchema> Tables { get; set; }
        // the version of the contract. this value changes if the schema changes in the database
        // and a new contract needs to be issued to participants. in that case, the contractGUID is the same,
        // but the version changes
        public Guid Version { get; set; }
        // the current state of the contract
        public ContractStatus Status { get; set; }
    }

    // the actual object sent to the Participant 
    // showing the contract information and the 
    // participant id generated by the host
    internal class ContractInfo
    {
        public Contract Contract { get; set; }
        public Participant Participant { get; set; }
    }
}
