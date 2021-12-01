using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.SQLParsing
{
    public readonly struct SyntaxError
    {
        public readonly IRecognizer Recognizer;
        public readonly IToken OffendingSymbol;
        public readonly int Line;
        public readonly int CharPositionInLine;
        public readonly string Message;
        public readonly RecognitionException Exception;

        public SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line,
                           int charPositionInLine, string message, RecognitionException exception)
        {
            Recognizer = recognizer;
            OffendingSymbol = offendingSymbol;
            Line = line;
            CharPositionInLine = charPositionInLine;
            Message = message;
            Exception = exception;
        }
    }

    public class SyntaxErrorListener : BaseErrorListener
    {
        public readonly List<SyntaxError> Errors = new List<SyntaxError>();
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            Errors.Add(new SyntaxError(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }
    }
}
