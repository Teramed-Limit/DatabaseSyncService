namespace DatabaseSyncService.Model;

public class DatabaseSettings
{
    public DatabaseConnectionSettings SourceDBSettings { get; set; }
    public DatabaseConnectionSettings TargetDBSettings { get; set; }
    public TableSyncMode TableSyncMode { get; set; }
    public List<string> TableNames { get; set; }
    public bool BackupToTargetDB { get; set; }
    public int ChangeRetentionDays { get; set; }
    public int SyncIntervalSeconds { get; set; }
}

public class DatabaseConnectionSettings
{
    public string ServerName { get; set; }
    public string DatabaseName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public enum TableSyncMode
{
    All,
    Include,
    Exclude
}