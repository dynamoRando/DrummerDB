using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a = Antlr4.Runtime.Misc;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.SQLParsing
{
    /// <summary>
    /// Utility functions for interacting with Antlr's contexts
    /// </summary>
    static class ParserUtil
    {
        public static string GetWhiteSpaceFromCurrentContext(ParserRuleContext context, ICharStream stream)
        {
            int a = context.Start.StartIndex;
            int b = context.Stop.StopIndex;
            a.Interval interval = new a.Interval(a, b);
            stream = context.Start.InputStream;
            return stream.GetText(interval);
        }

        public static string GetWhitespaceStringFromTokenInterval(a.Interval interval, CommonTokenStream tokenStream, ICharStream stream)
        {
            try
            {
                var start = tokenStream.Get(interval.a).StartIndex;
                var end = tokenStream.Get(interval.b).StopIndex;
                a.Interval i = new a.Interval(start, end);
                return stream.GetText(i);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
