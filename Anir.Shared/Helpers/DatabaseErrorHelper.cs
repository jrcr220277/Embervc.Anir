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
        /// <summary>
        /// Obtiene la excepción más interna (InnerException),
        /// porque EF Core suele envolver los errores en DbUpdateException.
        /// </summary>
        private static Exception GetInnermost(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        /// <summary>
        /// Clasifica la excepción en un tipo conocido.
        /// Devuelve un string que luego el middleware traduce en mensajes claros.
        /// </summary>
        public static string Classify(Exception ex)
        {
            var inner = GetInnermost(ex);
            var msg = inner.Message ?? string.Empty;

            // ============================================================
            // POSTGRESQL
            // ============================================================
            if (msg.Contains("23505")) return "UniqueViolation";        // unique_violation
            if (msg.Contains("23503")) return "ForeignKeyViolation";    // foreign_key_violation
            if (msg.Contains("23502")) return "NotNullViolation";       // not_null_violation
            if (msg.Contains("40P01")) return "Deadlock";               // deadlock_detected
            if (msg.Contains("22001")) return "StringTooLong";          // string_data_right_truncation
            if (msg.Contains("22003")) return "NumericOutOfRange";      // numeric_value_out_of_range
            if (msg.Contains("22007")) return "InvalidDateTime";        // invalid_datetime_format

            // Errores adicionales comunes en PostgreSQL
            if (msg.Contains("42883")) return "UndefinedFunction";      // undefined_function
            if (msg.Contains("42804")) return "DatatypeMismatch";       // datatype_mismatch
            if (msg.Contains("22021")) return "InvalidEncoding";        // invalid byte sequence
            if (msg.Contains("XX000")) return "InternalDatabaseError";  // internal error

            // ============================================================
            // SQL SERVER
            // ============================================================
            if (msg.Contains("2627") || msg.Contains("2601")) return "UniqueViolation"; // duplicate key
            if (msg.Contains("547")) return "ForeignKeyViolation";       // FK violation
            if (msg.Contains("515")) return "NotNullViolation";          // cannot insert null
            if (msg.Contains("1205")) return "Deadlock";                 // deadlock victim
            if (msg.Contains("8115")) return "ArithmeticOverflow";       // overflow
            if (msg.Contains("245")) return "ConversionError";           // conversion failed

            // ============================================================
            // MYSQL
            // ============================================================
            if (msg.Contains("1062")) return "DuplicateEntry";           // duplicate entry
            if (msg.Contains("1452")) return "ForeignKeyViolation";      // FK violation
            if (msg.Contains("1048")) return "NotNullViolation";         // column cannot be null
            if (msg.Contains("1213")) return "Deadlock";                 // deadlock
            if (msg.Contains("1264")) return "OutOfRangeValue";          // out of range
            if (msg.Contains("1406")) return "DataTooLong";              // data too long

            // ============================================================
            // GENÉRICOS
            // ============================================================
            if (inner is TimeoutException) return "Timeout";

            return "Unknown";
        }
    }
}
