using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a = Antlr4.Runtime.Misc;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class ContextWrapper
    {
        private ParserRuleContext _context;
        private ICharStream _charStream;

        public ParserRuleContext Context => _context;
        public string Debug => Context.GetText();
        public string FullText => GetWhiteSpaceFromCurrentContext(Context);

        public ContextWrapper(ParserRuleContext context, ICharStream stream)
        {
            _context = context;
            _charStream = stream;
        }

        private string GetWhiteSpaceFromCurrentContext(ParserRuleContext context)
        {
            int a = context.Start.StartIndex;
            int b = context.Stop.StopIndex;
            a.Interval interval = new a.Interval(a, b);
            _charStream = context.Start.InputStream;
            return _charStream.GetText(interval);
        }
    }
}
