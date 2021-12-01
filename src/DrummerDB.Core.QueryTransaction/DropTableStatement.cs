using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class DropTableStatement : IStatement, IDDLStatement
    {
        private IDatabase _db;

        public StatementType Type => StatementType.DDL;
        public string TableName { get; set; }
        public string FullText { get; set; }
        public bool CheckTableExists { get; set; }
        public bool IsValidated { get; set; }

        public DropTableStatement(string fullText, IDatabase db)
        {
            FullText = fullText;
            _db = db;
            ParseText();
        }

        private void ParseText()
        {
            string text = FullText.Replace($"{DDLKeywords.DROP} {DDLKeywords.TABLE} ", string.Empty);

            if (text.Contains($"{DDLKeywords.IF_EXISTS}"))
            {
                CheckTableExists = true;
            }
            else
            {
                CheckTableExists = false;
            }

            text = text.Replace(DDLKeywords.IF_EXISTS, string.Empty).Trim();
            text = text.Replace(";", string.Empty);
            TableName = text;

            if (_db.HasTable(TableName))
            {
                IsValidated = true;
            }
            else
            {
                IsValidated = false;
            }
        }
    }
}
