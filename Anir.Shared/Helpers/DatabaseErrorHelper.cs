using System;

namespace Anir.Shared.Helpers
{
    /// <summary>
    /// Helper centralizado para clasificar excepciones de base de datos.
    /// Compatible con PostgreSQL, SQL Server y MySQL.
    /// No depende de EF Core, por lo que puede vivir en Shared.
    /// </summary>
    public static class DatabaseErrorHelper
    {
        private static Exception GetInnermost(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        public static string Classify(Exception ex)
        {
            var inner = GetInnermost(ex);
            var msg = inner.Message ?? string.Empty;

            // Normalizamos el mensaje para evitar problemas de mayúsculas/minúsculas
            msg = msg.ToLowerInvariant();

            // ============================================================
            // POSTGRESQL
            // ============================================================
            // UNIQUE
            if (msg.Contains("23505")) return "UniqueViolation";

            // FOREIGN KEY (dos variantes)
            if (msg.Contains("23503")) return "ForeignKeyViolation"; // foreign_key_violation
            if (msg.Contains("23001")) return "ForeignKeyViolation"; // restrict_violation

            // NOT NULL
            if (msg.Contains("23502")) return "NotNullViolation";

            // DEADLOCK
            if (msg.Contains("40p01")) return "Deadlock";

            // STRING TOO LONG
            if (msg.Contains("22001")) return "StringTooLong";

            // NUMERIC OUT OF RANGE
            if (msg.Contains("22003")) return "NumericOutOfRange";

            // INVALID DATETIME
            if (msg.Contains("22007")) return "InvalidDateTime";

            // Otros comunes
            if (msg.Contains("42883")) return "UndefinedFunction";
            if (msg.Contains("42804")) return "DatatypeMismatch";
            if (msg.Contains("22021")) return "InvalidEncoding";
            if (msg.Contains("xx000")) return "InternalDatabaseError";

            // ============================================================
            // SQL SERVER
            // ============================================================
            // UNIQUE
            if (msg.Contains("2627") || msg.Contains("2601")) return "UniqueViolation";

            // FOREIGN KEY
            if (msg.Contains("547")) return "ForeignKeyViolation";

            // NOT NULL
            if (msg.Contains("515")) return "NotNullViolation";

            // DEADLOCK
            if (msg.Contains("1205")) return "Deadlock";

            // NUMERIC OVERFLOW
            if (msg.Contains("8115")) return "ArithmeticOverflow";

            // CONVERSION ERROR
            if (msg.Contains("245")) return "ConversionError";

            // ============================================================
            // MYSQL
            // ============================================================
            // UNIQUE
            if (msg.Contains("1062")) return "UniqueViolation";

            // FOREIGN KEY
            if (msg.Contains("1452")) return "ForeignKeyViolation";

            // NOT NULL
            if (msg.Contains("1048")) return "NotNullViolation";

            // DEADLOCK
            if (msg.Contains("1213")) return "Deadlock";

            // OUT OF RANGE
            if (msg.Contains("1264")) return "OutOfRangeValue";

            // STRING TOO LONG
            if (msg.Contains("1406")) return "StringTooLong";

            // ============================================================
            // GENÉRICOS
            // ============================================================
            if (inner is TimeoutException) return "Timeout";

            // ============================================================
            // FALLBACK
            // ============================================================
            return "Unknown";
        }
    }
}
