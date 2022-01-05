using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.SQLService;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Structures;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Common.Communication.SQLService.SQLService;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class DrummerSQLService : SQLServiceBase
    {
        private LogService _logger;
        private SQLServiceHandler _handler;

        public DrummerSQLService(LogService logger, SQLServiceHandler handler)
        {
            _logger = logger;
            _handler = handler;
        }

        public override Task<SQLQueryReply> ExecuteSQLQuery(SQLQueryRequest request, ServerCallContext context)
        {
            var reply = new SQLQueryReply();
            var resultSet = new SQLResultset();
            var authResult = new AuthResult();

            try
            {
                string userName = request.Authentication.UserName;
                string databaseName = request.DatabaseName;
                string pw = request.Authentication.Pw;
                string statement = request.SqlStatement;
                DatabaseType type = (DatabaseType)request.DatabaseType;



                Resultset result = null;
                Guid userSessionId;

                bool hasSessionId = Guid.TryParse(request.UserSessionId, out userSessionId);
                bool userIsAuthorized = false;

                string errorMessage = string.Empty;

                if (_handler.UserHasRights(userName, pw))
                {
                    userIsAuthorized = true;
                    if (string.IsNullOrEmpty(databaseName))
                    {
                        if (_handler.IsValidQuery(statement, userName, pw, type, out errorMessage))
                        {
                            if (hasSessionId)
                            {
                                result = _handler.ExecuteQuery(statement, userName, pw, databaseName, userSessionId, type);
                            }
                        }
                    }
                    else
                    {
                        if (_handler.IsValidQuery(statement, userName, pw, databaseName, type, out errorMessage))
                        {
                            if (hasSessionId)
                            {
                                result = _handler.ExecuteQuery(statement, userName, pw, databaseName, userSessionId, type);
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
                                var rowMetaData = new Common.Communication.RowRemoteMetadata();
                                row.RemoteMetadata = rowMetaData;

                                for (int i = 0; i < numberOfColumns; i++)
                                {
                                    var currentValue = rRow[i];

                                    if (currentValue.IsRemotable)
                                    {
                                        row.IsRemoteable = true;
                                        row.RemoteMetadata.IsHashOutOfSyncWithHost = currentValue.IsHashOutOfSyncWithHost;
                                        row.RemoteMetadata.IsRemoteOutOfSyncWithHost = currentValue.IsRemoteOutOfSyncWithHost;
                                    }

                                    if (!currentValue.IsRemoteDeleted)
                                    {
                                        var rowValue = new Common.Communication.RowValue();
                                        rowValue.Value = ByteString.CopyFrom(currentValue.Value);
                                        rowValue.IsNullValue = currentValue.IsNullValue;
                                        rowValue.Column = new Common.Communication.ColumnSchema();
                                        rowValue.Column.ColumnName = result.Columns[i].Name;
                                        rowValue.Column.ColumnType = Convert.ToUInt32(result.Columns[i].DataType);
                                        rowValue.Column.IsNullable = result.Columns[i].IsNullable;
                                        rowValue.Column.ColumnLength = Convert.ToUInt32(result.Columns[i].Length);
                                        row.Values.Add(rowValue);
                                    }
                                    else
                                    {
                                        row.RemoteMetadata.IsRemoteDeleted = true;
                                        row.RemoteMetadata.RemoteDeletedDate = Timestamp.FromDateTime(currentValue.RemoteDeletedDateUTC);
                                    }
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
                            resultSet.IsError = true;
                        }
                    }

                    // need to transform the result into a SQLQueryReply
                    reply.Results.Add(resultSet);
                    authResult.IsAuthenticated = userIsAuthorized;
                    reply.AuthenticationResult = authResult;
                }
                else
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        resultSet.ExecutionErrorMessage = errorMessage;
                        resultSet.IsError = true;
                        reply.Results.Add(resultSet);
                    }

                    if (!userIsAuthorized)
                    {
                        authResult.IsAuthenticated = false;
                        reply.AuthenticationResult = authResult;
                    }
                }

                // santiy check
                if (reply.Results.Count == 0)
                {
                    reply.Results.Add(resultSet);
                }
            }
            catch (Exception ex)
            {
                if (_logger is not null)
                {
                    _logger.Error(ex, "Error in SQL Service");
                    Debug.WriteLine(ex.ToString());
                    Console.WriteLine(ex.ToString());
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

        public void SetLogger(LogService logger)
        {
            _logger = logger;
        }
    }
}
