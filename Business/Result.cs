using Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Business
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "Successful";
        public object? Data { get; set; }

        public Result() { }

        public Result(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        // 🔴 MUST be static
        public static Result DBcommit(
            LMSContext context,
            string successMessage,
            string? failedMessage = null,
            object? data = null)
        {
            try
            {
                context.SaveChanges();
                var conn = context.Database.GetDbConnection();
                Console.WriteLine($"DB: {conn.Database} | Server: {conn.DataSource}");
                return new Result(true, successMessage, data);
            }
            catch (Exception ex)
            {
                var msg = failedMessage ?? ex.Message;
                if (ex.InnerException != null)
                    msg += " | Inner: " + ex.InnerException.Message;

                return new Result(false, msg);
            }
        }
    }
}
