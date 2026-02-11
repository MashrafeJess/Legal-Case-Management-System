using Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Business
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "Successful";
        public object? Data { get; set; }
        public string? ErrorCode { get; set; }
        public string? StackTrace { get; set; }

        public Result() { }

        public Result(
            bool success,
            string message,
            object? data = null,
            string? errorCode = null,
            string? stackTrace = null)
        {
            Success = success;
            Message = message;
            Data = data;
            ErrorCode = errorCode;
            StackTrace = stackTrace;
        }

        // ✅ ASYNC + SAFE + DEBUG FRIENDLY
        public static async Task<Result> DBCommitAsync(
            LMSContext context,
            string successMessage,
            ILogger? logger = null,
            string? failedMessage = null,
            object? data = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);

                DbConnection conn = context.Database.GetDbConnection();

                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger?.LogInformation(
                    "DB Commit Success | Database: {Database} | Server: {Server}",
                    conn.Database,
                    conn.DataSource
                    );
                }

                return new Result(true, successMessage, data);
            }
            catch (DbUpdateException dbEx)
            {
                var msg = failedMessage ?? "Database update failed";

                logger?.LogError(dbEx,
                    "DB Update Exception | Message: {Message}",
                    dbEx.Message);

                return new Result(
                    false,
                    msg,
                    null,
                    errorCode: "DB_UPDATE_ERROR",
                    stackTrace: dbEx.StackTrace
                );
            }
            catch (OperationCanceledException)
            {
                logger?.LogWarning("DB Commit cancelled");

                return new Result(
                    false,
                    "Operation cancelled",
                    errorCode: "OPERATION_CANCELLED"
                );
            }
            catch (Exception ex)
            {
                var msg = failedMessage ?? "Critical system error";

                logger?.LogCritical(ex,
                    "CRITICAL ERROR during DB Commit");

                return new Result(
                    false,
                    msg,
                    null,
                    errorCode: "CRITICAL_ERROR",
                    stackTrace: ex.StackTrace
                );
            }
        }
    }
}
