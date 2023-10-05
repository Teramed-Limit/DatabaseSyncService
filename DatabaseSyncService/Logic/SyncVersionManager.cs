using System.Data.SqlClient;
using DatabaseSyncService.Helper;
using DatabaseSyncService.Model;
using Microsoft.Extensions.Options;

namespace DatabaseSyncService.Logic;

public class SyncVersionManager
{
    private const string VersionFileFormat = "{0}.txt"; // {0} is the table name

    private readonly DatabaseSettings _databaseSettings;

    public SyncVersionManager(IOptions<DatabaseSettings> settings)
    {
        if (!Directory.Exists("SyncVersions"))
            Directory.CreateDirectory("SyncVersions");
        _databaseSettings = settings.Value;
    }

    private string GetVersionFilePath(string tableName)
    {
        return string.Format(VersionFileFormat, tableName);
    }

    public long GetLastSyncVersion(string tableName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "SyncVersions", GetVersionFilePath(tableName));

        if (File.Exists(filePath))
        {
            var versionText = File.ReadAllText(filePath);
            if (long.TryParse(versionText, out var version))
            {
                return version;
            }
        }

        return 0; // 預設的版本號為0，或您可以返回一個其他的初始值
    }

    public void UpdateSyncVersion(string tableName, long newVersion)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "SyncVersions", GetVersionFilePath(tableName));
        File.WriteAllText(filePath, newVersion.ToString());
    }

    public long GetCurrentChangeTrackingVersion()
    {
        var sourceConnectionString = DatabaseHelper.ToConnectionString(_databaseSettings.SourceDBSettings);
        using var connection = new SqlConnection(sourceConnectionString);
        connection.Open();

        using var cmd = new SqlCommand("SELECT CHANGE_TRACKING_CURRENT_VERSION()", connection);
        return (long)cmd.ExecuteScalar();
    }
}