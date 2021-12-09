using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures.Enum;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateHostDbStatement : IStatement, IDDLStatement
    {
        public StatementType Type => StatementType.DDL;
        public bool IsValidated { get; set; }
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public string FullText { get; set; }

        public CreateHostDbStatement(string fullText)
        {
            FullText = fullText;
            ParseText();
        }

        private void ParseText()
        {
            string dbName = FullText.Replace("CREATE DATABASE ", string.Empty);
            dbName = dbName.Trim();
            DatabaseName = dbName;
        }
    }
}
