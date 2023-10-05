using System.Data.Common;
using System.Dynamic;
using DatabaseSyncService.Helper;
using RepoDb;

namespace DatabaseSyncService.SqlExecutor;

public class EnableTableCTExecutor : RawSqlExecutor
{
    public EnableTableCTExecutor(DbConnection connection)
        : base(connection)
    {
    }

    public override IEnumerable<dynamic> Execute(Dictionary<string, object> parameters = null)
    {
        Open();

        // 檢查parameters
        if (parameters == null || !parameters.ContainsKey("tableName"))
            throw new ArgumentException("parameters must contain tableName");

        var tableName = parameters["tableName"].ToString();

        // ENABLE CHANGE_TRACKING: 這部分的指令將該表的Change Tracking功能設定為啟用狀態。啟用後，你可以追踪這個表中的資料更改，如新增、修改和刪除的記錄。
        // TRACK_COLUMNS_UPDATED = ON: 只處理異動的欄位，而不是整個資料表。
        // 當此選項啟用時，可以透過SYS_CHANGE_COLUMNS這個系統函數取得那些列被更新。
        var sql = $@"IF NOT EXISTS(
                        SELECT 1 
                        FROM sys.change_tracking_tables 
                        WHERE object_id = OBJECT_ID('{tableName}')
                    )
                    BEGIN
                        ALTER TABLE {tableName}
                        ENABLE CHANGE_TRACKING
                    END";
        _connection.ExecuteNonQuery(sql);
        Close();
        return null;
    }
}