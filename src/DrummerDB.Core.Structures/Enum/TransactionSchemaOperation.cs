namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    enum TransactionSchemaOperation
    {
        CreateTable,
        CreateHostDatabase,
        CreatePartDatabase,
        CreateEmbeddedDatabase,
        CreateTenantDatabase,
        AlterTable,
        AlterColumn,
        DropTable,
        DropHostDatabase,
        DropPartDatabase,
        DropEmbeddedDatabase,
        DropTenantDatabase,
        Unknown,
        ParticipantSaveContract,
        NotifyAcceptContract,
        ReceivedContractAcceptance
    }
}
