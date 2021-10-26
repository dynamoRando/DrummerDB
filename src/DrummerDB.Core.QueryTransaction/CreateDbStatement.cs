using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class CreateDbStatement : IStatement, IDDLStatement
    {
        public StatementType Type => StatementType.DDL;
        public bool IsValidated { get; set; }
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType {  get; set; }
        public string FullText { get; set; }

        public CreateDbStatement(string fullText)
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
