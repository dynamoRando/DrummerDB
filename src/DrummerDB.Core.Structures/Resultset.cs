using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class Resultset
    {
        public ColumnSchemaStruct[] Columns { get; set; }
        public List<ResultsetValue[]> Rows { get; set; }
        public List<string> ExecutionErrors { get; set; }
        public List<string> AuthenticationErrors { get; set; }
        public List<string> NonQueryMessages { get; set; }

        public Resultset()
        {
            Rows = new List<ResultsetValue[]>();
            ExecutionErrors = new List<string>();
            AuthenticationErrors = new List<string>();
            NonQueryMessages = new List<string>();
        }

        public bool HasExecutionErrors()
        {
            return ExecutionErrors.Any();
        }

        public bool HasAuthenticationErrors()
        {
            return AuthenticationErrors.Any();
        }

        public bool HasNonQueryMessages()
        {
            return NonQueryMessages.Any();
        }
    }
}
