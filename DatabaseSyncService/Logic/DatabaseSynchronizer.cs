using System.Reflection;
using DatabaseSyncService.SqlExecutor;

namespace DatabaseSyncService.Logic
{
    public class DatabaseSynchronizer
    {
        private readonly ExecutorFactory _executorFactory;
        private readonly SyncVersionManager _syncVersionManager;
        private readonly RepoDbWriter _sourceRepoDbWriter;
        private readonly RepoDbWriter _targetRepoDbWriter;
        private readonly ILogger<Worker> _logger;

        public DatabaseSynchronizer(
            ExecutorFactory executorFactory,
            SyncVersionManager syncVersionManager,
            IServiceProvider serviceProvider,
            ILogger<Worker> logger)
        {
            _executorFactory = executorFactory;
            _syncVersionManager = syncVersionManager;
            _logger = logger;
            _sourceRepoDbWriter = serviceProvider.GetRequiredService<RepoDbWriter>();
            _sourceRepoDbWriter.Connect("SourceConnection");
            _targetRepoDbWriter = serviceProvider.GetRequiredService<RepoDbWriter>();
            _targetRepoDbWriter.Connect("TargetConnection");
        }

        public void Synchronize(string tableName)
        {
            try
            {
                // 取得上次同步的版本號
                var lastSyncVersion = _syncVersionManager.GetLastSyncVersion(tableName);

                // 建立參數以進行新增/更新操作
                var parameters = CreateParameters(tableName, lastSyncVersion, "Merge");
                var dataToSync = ExecuteQuery("SourceConnection", parameters);
                HandleUpsertOperation(tableName, dataToSync);

                // 建立參數以進行刪除操作
                parameters = CreateParameters(tableName, lastSyncVersion, "Delete");
                dataToSync = ExecuteQuery("SourceConnection", parameters);
                HandleDeleteOperation(tableName, dataToSync);

                // 一旦同步完成，更新last_sync_version
                UpdateSyncVersion(tableName);
            }
            finally
            {
                CloseConnections();
            }
        }

        private Dictionary<string, object> CreateParameters(string tableName, long lastSyncVersion, string operation)
        {
            return new Dictionary<string, object>
            {
                { "tableName", tableName },
                { "lastSyncVersion", lastSyncVersion },
                { "operation", operation },
            };
        }

        private IEnumerable<dynamic> ExecuteQuery(string connectionName, Dictionary<string, object> parameters)
        {
            var queryExecutor = _executorFactory.CreateQueryCTExecutor(connectionName); // Create query executor
            return queryExecutor.Execute(parameters); // Execute query
        }

        private void HandleUpsertOperation(string tableName, IEnumerable<dynamic> dataToSync)
        {
            foreach (IDictionary<string, object> dataDict in dataToSync)
            {
                var dataDictCopy = new Dictionary<string, object>(dataDict);
                dataDictCopy.Remove("SYS_CHANGE_COLUMNS");
                _targetRepoDbWriter.Upsert(tableName, dataDictCopy);
            }
        }

        private void HandleDeleteOperation(string tableName, IEnumerable<dynamic> dataToSync)
        {
            foreach (IDictionary<string, object> dataDict in dataToSync)
            {
                var dataDictCopy = new Dictionary<string, object>(dataDict);
                _targetRepoDbWriter.Delete(tableName, dataDictCopy);
            }
        }

        private void UpdateSyncVersion(string tableName)
        {
            var currentVersion = _syncVersionManager.GetCurrentChangeTrackingVersion();
            _syncVersionManager.UpdateSyncVersion(tableName, currentVersion);
        }

        private void CloseConnections()
        {
            _sourceRepoDbWriter.Close();
            _targetRepoDbWriter.Close();
        }
    }
}