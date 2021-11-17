using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents an entry in the log file of an action that occured in the database
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Storage.Interface.ITransactionEntry" />
    internal class TransactionEntry : ITransactionEntry
    {
        #region Private Fields
        int _V100 = Constants.DatabaseVersions.V100;
        private bool _isCompleted;
        private DateTime _completedTimeUTC;
        private bool _isDeleted;
        private int _sequenceNumber;
        private TransactionEntryKey _key;
        #endregion

        #region Public Properties
        public bool IsCompleted => _isCompleted;
        public int BinarySize => GetBinaryLength();
        public byte[] BinaryData => GetBinaryData();
        public DateTime CompletedTimeUTC => _completedTimeUTC;
        public bool IsDeleted => _isDeleted;
        public TransactionEntryKey Key => _key;

        public readonly Guid TransactionBatchId;
        public readonly Guid AffectedObjectId;
        public readonly DateTime EntryTimeUTC;
        public readonly TransactionActionType ActionType;
        public readonly int TransactionActionVersion;
        public readonly int ActionBinaryLength;
        public ITransactionBinary Action;
        public int UserNameLength;
        public string UserName;
        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionEntry"/> class.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>Use when constructing an entry from disk</remarks>
        public TransactionEntry(ReadOnlySpan<byte> preamble)
        {
            _isCompleted = DbBinaryConvert.BinaryToBoolean
                (preamble.Slice(TransactionConstants.IsCompletedOffset(_V100),
                TransactionConstants.SIZE_OF_IS_COMPLETED(_V100)));

            _sequenceNumber = DbBinaryConvert.BinaryToInt
                (preamble.Slice(TransactionConstants.TransactionSequenceOffset(_V100),
                TransactionConstants.SIZE_OF_TRANSACTION_SEQUENCE(_V100)));

            TransactionBatchId = DbBinaryConvert.BinaryToGuid
               (preamble.Slice(TransactionConstants.TransactionBatchIdOffset(_V100),
               TransactionConstants.SIZE_OF_TRANSACTION_BATCH_ID(_V100)));

            AffectedObjectId = DbBinaryConvert.BinaryToGuid
             (preamble.Slice(TransactionConstants.AffectedObjectIdOffset(_V100),
             TransactionConstants.SIZE_OF_AFFECTED_OBJECT_ID(_V100)));

            EntryTimeUTC = DbBinaryConvert.BinaryToDateTime
           (preamble.Slice(TransactionConstants.EntryTimeUTCOffset(_V100),
           TransactionConstants.SIZE_OF_ENTRY_TIME_UTC(_V100)));

            _completedTimeUTC = DbBinaryConvert.BinaryToDateTime
           (preamble.Slice(TransactionConstants.CompletedTimeUTCOffset(_V100),
           TransactionConstants.SIZE_OF_COMPLETED_TIME_UTC(_V100)));

            ActionType = (TransactionActionType)DbBinaryConvert.BinaryToInt
           (preamble.Slice(TransactionConstants.TransactionActionTypeOffset(_V100),
           TransactionConstants.SIZE_OF_TRANSACTION_ACTION_TYPE(_V100)));

            TransactionActionVersion = DbBinaryConvert.BinaryToInt
           (preamble.Slice(TransactionConstants.TransactionActionVersionOffset(_V100),
           TransactionConstants.SIZE_OF_TRANSACTION_ACTION_VERSION(_V100)));

            ActionBinaryLength = DbBinaryConvert.BinaryToInt
           (preamble.Slice(TransactionConstants.ActionBinaryLengthOffset(_V100),
           TransactionConstants.SIZE_OF_ACTION_BINARY_LENGTH(_V100)));

            _isDeleted = DbBinaryConvert.BinaryToBoolean
          (preamble.Slice(TransactionConstants.IsDeletedOffset(_V100),
          TransactionConstants.SIZE_OF_IS_DELETED(_V100)));

            UserNameLength = DbBinaryConvert.BinaryToInt
          (preamble.Slice(TransactionConstants.UserNameLengthOffset(_V100),
          TransactionConstants.SIZE_OF_USER_NAME_LENGTH(_V100)));

            _key = new TransactionEntryKey { SequenceNumber = _sequenceNumber, TransactionBatchId = TransactionBatchId };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionEntry"/> class with a <see cref="TransactionEntry.IsCompleted"/> status of <c>FALSE</c>
        /// </summary>
        /// <param name="transactionBatchId">The transaction batch identifier.</param>
        /// <param name="affectedObjectId">The affected object identifier.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="transactionActionVersion">The transaction action version.</param>
        /// <param name="action">The action.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="queryPlan">The query plan.</param>
        /// <remarks>Use when constructing a new object to save to disk</remarks>
        public TransactionEntry
            (
                Guid transactionBatchId,
                Guid affectedObjectId,
                TransactionActionType actionType,
                int transactionActionVersion,
                ITransactionBinary action,
                string userName,
                bool isDeleted,
                int sequenceNumber
            )
        {
            TransactionBatchId = transactionBatchId;
            AffectedObjectId = affectedObjectId;
            EntryTimeUTC = DateTime.UtcNow;
            ActionType = actionType;
            Action = action;
            UserName = userName;
            TransactionActionVersion = transactionActionVersion;
            _isCompleted = false;
            ActionBinaryLength = action.GetDataInTransactionBinaryFormat().Length;
            _isDeleted = isDeleted;
            _sequenceNumber = sequenceNumber;
            _key = new TransactionEntryKey { SequenceNumber = _sequenceNumber, TransactionBatchId = transactionBatchId };
            UserNameLength = userName.Length;

            // TODO - need to figure out how to express an entry
            // typically, you do something to an object
            // i.e. read data from it, or alter it, or update it
            // and you have data related to the action, i.e.
            // the columns you altered on the table
            // or the row(s) that you inserted into a table
            // etc

            /*
             * If we save this to disk, we will need an indicator to specify the action type (so an enum)
             * And then we need to save off a byte length for the entire entry if we are saving this in binary format.
             */

        }

        #endregion

        #region Public Methods
        public void MarkDeleted()
        {
            _isDeleted = true;
        }
        public void MarkComplete()
        {
            _isCompleted = true;
            _completedTimeUTC = DateTime.UtcNow;
        }

        public void MarkIncomplete()
        {
            _isCompleted = false;
        }
        public void SetActionFromBinary(ReadOnlySpan<byte> action)
        {
            int currentOffset = 0;
            if (ActionType == TransactionActionType.Data)
            {
                // Operation
                // Address
                // Data

                // Operation is an ENUM/INT, so 4 bytes
                int operationType = DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));
                TransactionDataOperation operation = (TransactionDataOperation)operationType;

                currentOffset += Constants.SIZE_OF_INT;

                // SQLAddress is a struct, that consists of 
                // DatabaseId - GUID
                // TableId - INT
                // PageId - INT
                // RowId - INT
                // RowOffset - INT

                Guid dbId;
                dbId = DbBinaryConvert.BinaryToGuid(action.Slice(currentOffset, Constants.SIZE_OF_GUID));

                currentOffset += Constants.SIZE_OF_GUID;

                int tableId;
                tableId = DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));

                currentOffset += Constants.SIZE_OF_INT;

                int pageId;
                pageId = DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));

                currentOffset += Constants.SIZE_OF_INT;

                int rowId;
                rowId = DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));

                currentOffset += Constants.SIZE_OF_INT;

                int rowOffset;
                rowOffset = DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));

                currentOffset += Constants.SIZE_OF_INT;

                Guid schemaId;
                schemaId = DbBinaryConvert.BinaryToGuid(action.Slice(currentOffset, Constants.SIZE_OF_GUID));
                currentOffset += Constants.SIZE_OF_GUID;

                switch (operation)
                {
                    case TransactionDataOperation.Select:
                        throw new NotImplementedException();
                        break;
                    case TransactionDataOperation.Insert:
                        var dataOperation = new InsertTransaction(dbId, tableId, rowId, pageId, schemaId, action.Slice(currentOffset, action.Length - currentOffset));
                        Action = dataOperation;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown data operation type");

                }
            }

            if (ActionType == TransactionActionType.Permission)
            {
                throw new NotImplementedException();
            }

            if (ActionType == TransactionActionType.Schema)
            {
                TransactionSchemaOperation operation = (TransactionSchemaOperation)DbBinaryConvert.BinaryToInt(action.Slice(currentOffset, Constants.SIZE_OF_INT));

                currentOffset += Constants.SIZE_OF_INT;

                switch (operation)
                {
                    case TransactionSchemaOperation.CreateTable:
                        var tableSchema = new TableSchema(action.Slice(currentOffset, action.Length - currentOffset));
                        var tableOp = new CreateTableTransaction(tableSchema);
                        Action = tableOp;
                        break;
                    case TransactionSchemaOperation.CreateDatabase:
                        var bLengthOfDbName = action.Slice(currentOffset, Constants.SIZE_OF_INT);
                        int lengthOfDbName = DbBinaryConvert.BinaryToInt(bLengthOfDbName);
                        currentOffset += Constants.SIZE_OF_INT;
                        var bDbName = action.Slice(currentOffset, lengthOfDbName);
                        string dbName = DbBinaryConvert.BinaryToString(bDbName);
                        var createDb = new CreateDbTransaction(dbName); ;
                        Action = createDb;
                        currentOffset += lengthOfDbName;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown action type");
                }
            }

            if (ActionType == TransactionActionType.Unknown)
            {
                throw new NotImplementedException();
            }
        }

        public void SetUserNameLengthFromBinary(ReadOnlySpan<byte> data)
        {
            UserNameLength = DbBinaryConvert.BinaryToInt(data.Slice(0, TransactionConstants.SIZE_OF_USER_NAME_LENGTH(_V100)));
        }

        public void SetUserNameFromBinary(ReadOnlySpan<byte> data)
        {
            UserName = DbBinaryConvert.BinaryToString(data.Slice(0, UserNameLength));
        }

        public DropTableTransaction GetActionAsDropTable()
        {
            if (Action is DropTableTransaction)
            {
                return Action as DropTableTransaction;
            }

            return null;
        }

        public DeleteTransaction GetActionAsDelete()
        {
            if (Action is DeleteTransaction)
            {
                return Action as DeleteTransaction;
            }

            return null;
        }

        /// <summary>
        /// Attempts to cast the <see cref="Action"/> as an <see cref="InsertTransaction"/>, or NULL
        /// </summary>
        /// <returns>The <see cref="InsertTransaction"/>or NULL</returns>
        public InsertTransaction GetActionAsInsert()
        {
            if (Action is InsertTransaction)
            {
                return Action as InsertTransaction;
            }

            return null;
        }

        public UpdateTransaction GetActionAsUpdate()
        {
            if (Action is UpdateTransaction)
            {
                return Action as UpdateTransaction;
            }

            return null;
        }

        /// <summary>
        /// Attempts to cas the <see cref="Action"/> as an <see cref="SelectTableTransaction"/>, or NULL
        /// </summary>
        /// <returns>tHE <see cref="SelectTableTransaction"/> or NULL</returns>
        public SelectTableTransaction GetActionAsSelectTable()
        {
            if (Action is SelectTableTransaction)
            {
                return Action as SelectTableTransaction;
            }

            return null;
        }
        #endregion

        #region Private Methods
        private byte[] GetBinaryData()
        {
            byte[] actionBinaryData = Action.GetDataInTransactionBinaryFormat();
            byte[] actionBinaryDataLength = DbBinaryConvert.IntToBinary(actionBinaryData.Length);

            // preamble
            byte[] bIsCompleted = DbBinaryConvert.BooleanToBinary(IsCompleted.ToString());
            byte[] bSequence = DbBinaryConvert.IntToBinary(_sequenceNumber);
            byte[] bTransactionBatchId = DbBinaryConvert.GuidToBinary(TransactionBatchId);
            byte[] bTransactionActionType = DbBinaryConvert.IntToBinary(Convert.ToInt32(GetActionType()));
            byte[] bAffectedObjectId = DbBinaryConvert.GuidToBinary(AffectedObjectId);
            byte[] bEntryTimeUTC = DbBinaryConvert.DateTimeToBinary(EntryTimeUTC.ToString());
            byte[] bCompletedTimeUTC = DbBinaryConvert.DateTimeToBinary(CompletedTimeUTC.ToString());
            byte[] bUserNameLength = DbBinaryConvert.IntToBinary(UserName.Length);
            byte[] bUserName = DbBinaryConvert.StringToBinary(UserName);
            byte[] bTransactionVersion = DbBinaryConvert.IntToBinary(TransactionActionVersion);

            byte[] bIsDeleted = DbBinaryConvert.BooleanToBinary(IsDeleted);

            var arrays = new List<byte[]>();

            // preamble
            arrays.Add(bIsCompleted);
            arrays.Add(bSequence);
            arrays.Add(bTransactionBatchId);
            arrays.Add(bAffectedObjectId);
            arrays.Add(bEntryTimeUTC);
            arrays.Add(bCompletedTimeUTC);
            arrays.Add(bTransactionActionType);
            arrays.Add(bTransactionVersion);
            arrays.Add(actionBinaryDataLength);
            arrays.Add(bIsDeleted);
            arrays.Add(bUserNameLength);

            // variable data
            arrays.Add(actionBinaryData);
            arrays.Add(bUserName);

            return DbBinaryConvert.ArrayStitch(arrays);
        }

        private int GetBinaryLength()
        {
            return GetBinaryData().Length;
        }

        private TransactionActionType GetActionType()
        {
            switch (Action)
            {
                case TransactionActionData a:
                    return TransactionActionType.Data;
                case TransactionActionSchema b:
                    return TransactionActionType.Schema;
                default:
                    throw new InvalidOperationException("Unknown Transaction Action Type");
            }
        }
        #endregion
    }
}
