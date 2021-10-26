using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal static class DDLKeywords
    {
        private static string[] _keywords = new string[4];

        public const string CREATE = "CREATE";
        public const string ALTER = "ALTER";
        public const string DROP = "DROP";
        public const string TRUNCATE = "TRUNCATE";

        public static string[] Get()
        {
            _keywords[0] = CREATE;
            _keywords[1] = ALTER;
            _keywords[2] = DROP;
            _keywords[3] = TRUNCATE;
            return _keywords;
        }
    }
}
