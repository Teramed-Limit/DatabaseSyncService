using DatabaseSyncService.Logic;
using DatabaseSyncService.Model;
using Microsoft.Extensions.Options;

namespace DatabaseSyncService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChangeTrackingManager _trackingManager;
        private readonly TableSyncFilter _tableSyncFilter;
        private readonly DatabaseSettings _databaseSettings;

        public Worker(ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            IOptions<DatabaseSettings> settings,
            ChangeTrackingManager trackingManager,
            TableSyncFilter tableSyncFilter)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _trackingManager = trackingManager;
            _tableSyncFilter = tableSyncFilter;
            _databaseSettings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting database synchronization at: {time}", DateTimeOffset.Now);

                // 初始化 Change Tracking
                _trackingManager.EnableChangeTracking();

                // 如果設定為不同步，則不執行同步。
                // 主服務要定時同步到備份服務，當主服務發生異常時，備份服務可以立即啟動並提供服務。
                // 但不一定要啟動備份服務，所以這邊可以設定是否要同步。
                if (!_databaseSettings.BackupToTargetDB)
                    return;

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Wait for some time before the next synchronization.
                    // For example, this waits for 5 seconds.
                    await Task.Delay(TimeSpan.FromSeconds(_databaseSettings.SyncIntervalSeconds), stoppingToken);

                    // 遍歷每個表並同步
                    foreach (var tableName in _tableSyncFilter.FilterTablesBasedOnSyncMode())
                    {
                        // 執行同步
                        var synchronizer = _serviceProvider.GetRequiredService<DatabaseSynchronizer>();
                        synchronizer.Synchronize(tableName);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
                _logger.LogInformation("Finished database synchronization at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                await StartAsync(stoppingToken);
            }
        }
    }
}