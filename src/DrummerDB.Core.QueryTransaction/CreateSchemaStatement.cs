﻿using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class CreateSchemaStatement : IStatement, IDDLStatement
    {
        public StatementType Type => StatementType.DDL;
        public bool IsValidated { get; set; }
        public string FullText { get; set; }
        public string Name {  get; set; }   

        public CreateSchemaStatement(string fullText)
        {
            FullText = fullText;
            ParseText();
        }

        private void ParseText()
        {
            string schemaName = FullText.Replace("CREATE SCHEMA ", string.Empty);
            Name = schemaName.Trim();
        }
    }
}
