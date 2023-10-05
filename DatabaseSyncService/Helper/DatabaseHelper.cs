using System.Data.SqlClient;
using DatabaseSyncService.Model;

namespace DatabaseSyncService.Helper;

public static class DatabaseHelper
{
    public static string ToConnectionString(DatabaseConnectionSettings settings)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = settings.ServerName,
            InitialCatalog = settings.DatabaseName,
            UserID = settings.UserName,
            Password = settings.Password,
        };
        return builder.ConnectionString + ";TrustServerCertificate=true";
    }
}