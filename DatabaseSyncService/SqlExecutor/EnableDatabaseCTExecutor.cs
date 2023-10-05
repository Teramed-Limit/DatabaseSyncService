using System.Data.Common;
using System.Dynamic;
using DatabaseSyncService.Model;
using Microsoft.Extensions.Options;
using RepoDb;

namespace DatabaseSyncService.SqlExecutor;

public class EnableDatabaseCTExecutor : RawSqlExecutor
{
    private readonly DatabaseSettings _databaseSettings;

    public EnableDatabaseCTExecutor(DbConnection connection, DatabaseSettings settings)
        : base(connection)
    {
        _databaseSettings = settings;
    }

    public override IEnumerable<dynamic> Execute(Dictionary<string, object> parameters = null)
    {
        Open();
        // 檢查parameters
        if (parameters == null || !parameters.ContainsKey("databaseName"))
            throw new ArgumentException("parameters must contain tableName");

        var databaseName = parameters["databaseName"].ToString();

        // Enable Change Tracking at Database Level
        // 這個指令是將該資料庫的Change Tracking功能設定為啟用狀態。
        // Change Tracking是SQL Server提供的一種功能，允許你追踪資料庫表格中的更改（例如插入、更新或刪除），它是為了解決資料同步和更改追踪的問題而設計的。
        // CHANGE_RETENTION = 2 DAYS: 這意味著Change Tracking的資訊將被保留2天。換句話說，如果你在兩天之後查詢某些更改，那些更改的資訊可能已經被刪除。
        // AUTO_CLEANUP = ON: 這意味著過期的Change Tracking資料將會自動被清理。根據上面的設定，這將是每兩天一次。
        // https://learn.microsoft.com/zh-tw/sql/relational-databases/track-changes/enable-and-disable-change-tracking-sql-server?view=sql-server-ver16
        var sql = $@"IF NOT EXISTS(
                        SELECT 1 
                        FROM sys.change_tracking_databases 
                        WHERE database_id = DB_ID('{databaseName}')
                    )
                    BEGIN
                        ALTER DATABASE {databaseName}
                        SET CHANGE_TRACKING = ON
                        (CHANGE_RETENTION = {_databaseSettings.ChangeRetentionDays} DAYS, AUTO_CLEANUP = ON)
                    END";

        _connection.ExecuteNonQuery(sql);
        Close();
        return null;
    }
}