using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class DropDbStatement : IStatement, IDDLStatement
    {
        public StatementType Type => StatementType.DDL;
        public string DatabaseName { get; set; }
        public string FullText { get; set; }

        public bool IsValidated { get; set; }

        public DropDbStatement(string fullText)
        {
            FullText = fullText;
            ParseText();
        }

        private void ParseText()
        {
            string dbName = FullText.Replace("DROP DATABASE ", string.Empty);
            dbName = dbName.Trim();
            DatabaseName = dbName;
        }
    }
}
