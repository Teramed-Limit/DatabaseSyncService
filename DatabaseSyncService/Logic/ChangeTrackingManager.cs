using System.Data;
using DatabaseSyncService.Model;
using DatabaseSyncService.SqlExecutor;
using Microsoft.Extensions.Options;

namespace DatabaseSyncService.Logic;

public class ChangeTrackingManager
{
    private readonly ExecutorFactory _executorFactory;
    private readonly DatabaseSettings _databaseSettings;
    private readonly TableSyncFilter _tableSyncFilter;

    public ChangeTrackingManager(ExecutorFactory executorFactory, IOptions<DatabaseSettings> databaseSettings,
        TableSyncFilter tableSyncFilter)
    {
        _executorFactory = executorFactory;
        _tableSyncFilter = tableSyncFilter;
        _databaseSettings = databaseSettings.Value;
    }

    public void EnableChangeTracking()
    {
        var parameter = new Dictionary<string, object>
            { { "databaseName", _databaseSettings.SourceDBSettings.DatabaseName } };
        
        // 將該資料庫的Change Tracking功能設定為啟用狀態。
        _executorFactory.CreateEnableDatabaseCTExecutor("SourceConnection").Execute(parameter);

        // 為資料表啟用變更追蹤
        foreach (var tableName in _tableSyncFilter.FilterTablesBasedOnSyncMode())
        {
            parameter = new Dictionary<string, object> { { "tableName", tableName } };
            _executorFactory.CreateEnableTableCTExecutor("SourceConnection").Execute(parameter);
        }
    }
}