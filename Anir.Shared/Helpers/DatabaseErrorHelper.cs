namespace Anir.Shared.Helpers
{
    public static class DatabaseErrorHelper
    {
        public static bool IsUniqueViolation(Exception ex)
        {
            var msg = ex?.Message ?? string.Empty;
            return msg.Contains("23505") // Postgres
                || msg.Contains("2627") // SQL Server
                || msg.Contains("2601") // SQL Server
                || msg.Contains("1062"); // MySQL
        }

        public static bool IsForeignKeyViolation(Exception ex)
        {
            var msg = ex?.Message ?? string.Empty;
            return msg.Contains("547")   // SQL Server
                || msg.Contains("1452")  // MySQL
                || msg.Contains("23503"); // Postgres
        }

        public static bool IsNotNullViolation(Exception ex)
        {
            var msg = ex?.Message ?? string.Empty;
            return msg.Contains("515")   // SQL Server
                || msg.Contains("1048")  // MySQL
                || msg.Contains("23502"); // Postgres
        }

        public static bool IsDeadlock(Exception ex)
        {
            var msg = ex?.Message ?? string.Empty;
            return msg.Contains("1205")  // SQL Server
                || msg.Contains("1213")  // MySQL
                || msg.Contains("40P01"); // Postgres
        }
    }
}
