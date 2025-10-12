using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MiniUrl.Commons;

public class Utilities
{
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pgEx)
        {
            return pgEx.SqlState == PostgresErrorCodes.UniqueViolation;
        }

        return false;
    }
}