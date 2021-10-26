using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.SQLService;
using Drummersoft.DrummerDB.Core.Structures;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Common.Communication.SQLService.SQLService;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class DrummerSQLService : SQLServiceBase
    {
        private readonly ILogger<DrummerSQLService> _logger;
        private SQLServiceHandler _handler;

        public DrummerSQLService(ILogger<DrummerSQLService> logger, SQLServiceHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public override Task<SQLQueryReply> ExecuteSQLQuery(SQLQueryRequest request, ServerCallContext context)
        {
            string userName = request.Authentication.UserName;
            string databaseName = request.DatabaseName;
            string pw = request.Authentication.Pw;
            string statement = request.SqlStatement;

            var reply = new SQLQueryReply();
            var resultSet = new SQLResultset();
            var authResult = new AuthResult();

            Resultset result = null;
            Guid userSessionId;

            bool hasSessionId = Guid.TryParse(request.UserSessionId, out userSessionId);
            bool userIsAuthorized = false;

            string errorMessage = string.Empty;

            if (_handler.UserHasRights(userName, pw))
            {
                if (string.IsNullOrEmpty(databaseName))
                {
                    if (_handler.IsValidQuery(statement, userName, pw, out errorMessage))
                    {
                        if (hasSessionId)
                        {
                            result = _handler.ExecuteQuery(statement, userName, pw, databaseName, userSessionId);
                            userIsAuthorized = true;
                        }
                    }
                }
                else
                {
                    if (_handler.IsValidQuery(statement, userName, pw, databaseName, out errorMessage))
                    {
                        if (hasSessionId)
                        {
                            result = _handler.ExecuteQuery(statement, userName, pw, databaseName, userSessionId);
                            userIsAuthorized = true;
                        }
                    }
                }
            }
          
            if (result is not null)
            {
                if (!result.HasAuthenticationErrors() && !result.HasExecutionErrors())
                {
                    // succesful query with results
                    if (result.Rows.Count > 0)
                    {
                        int numberOfColumns = result.Columns.Length;

                        // is a successful query
                        resultSet.NumberOfRowsAffected = Convert.ToUInt32(result.Rows.Count);
                        foreach (var rRow in result.Rows)
                        {
                            var row = new Common.Communication.Row();

                            for (int i = 0; i < numberOfColumns; i++)
                            {
                                var rowValue = new Common.Communication.RowValue();
                                rowValue.Value = ByteString.CopyFrom(rRow[i].Value);

                                rowValue.Column = new Common.Communication.ColumnSchema();
                                rowValue.Column.ColumnName = result.Columns[i].Name;
                                rowValue.Column.ColumnType = Convert.ToUInt32(result.Columns[i].DataType);
                                rowValue.Column.IsNullable = result.Columns[i].IsNullable;
                                row.Values.Add(rowValue);
                            }

                            resultSet.Rows.Add(row);
                        }
                    }
                    else
                    {
                        // is a non query, like CREATE TABLE
                        resultSet.ResultMessage = result.NonQueryMessages.FirstOrDefault();
                    }
                }
                else
                {
                    // we had authentication or execution errors
                    if (result.HasAuthenticationErrors())
                    {
                        authResult.AuthenticationMessage = result.AuthenticationErrors.FirstOrDefault();
                        authResult.IsAuthenticated = false;
                    }

                    if (result.HasExecutionErrors())
                    {
                        resultSet.ExecutionErrorMessage = result.ExecutionErrors.FirstOrDefault();
                    }
                }

                // need to transform the reult into a SQLQueryReply
                reply.Results.Add(resultSet);
                authResult.IsAuthenticated = userIsAuthorized;
                reply.AuthenticationResult = authResult;
            }
            else
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    resultSet.ExecutionErrorMessage = errorMessage;
                }
            }

            return Task.FromResult(reply);
        }

        public override Task<TestReply> IsOnline(TestRequest request, ServerCallContext context)
        {
            _handler.HandleTest();
            var reply = new TestReply();
            reply.ReplyTimeUTC = DateTime.UtcNow.ToString();
            reply.ReplyEchoMessage = request.RequestEchoMessage;
            return Task.FromResult(reply);
        }
    }
}
